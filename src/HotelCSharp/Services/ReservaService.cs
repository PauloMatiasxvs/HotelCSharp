using HotelCSharp.Enums;
using HotelCSharp.Exceptions;
using HotelCSharp.Model;
using HotelCSharp.Repository.Interfaces;

namespace HotelCSharp.Services;

public sealed record RelatorioHotel(
    int ReservasAtivas,
    int ReservasFinalizadas,
    int ReservasCanceladas,
    decimal FaturamentoRealizado,
    decimal FaturamentoPrevisto,
    ETipoQuarto? TipoMaisReservado);

// Regras que envolvem mais de uma entidade (disponibilidade, relatório).
// Recebe os repositórios pela INTERFACE: funciona igual com memória ou Supabase.
public class ReservaService
{
    private readonly ICrudRepository<Reserva> _reservas;
    private readonly ICrudRepository<Quarto> _quartos;
    private readonly ICrudRepository<Hospede> _hospedes;

    public ReservaService(
        ICrudRepository<Reserva> reservas,
        ICrudRepository<Quarto> quartos,
        ICrudRepository<Hospede> hospedes)
    {
        _reservas = reservas;
        _quartos = quartos;
        _hospedes = hospedes;
    }

    public async Task<Reserva> ReservarAsync(int hospedeId, int quartoId, DateOnly entrada, DateOnly saida, int quantidadeHospedes)
    {
        var hospede = await _hospedes.ObterAsync(hospedeId) ?? throw new RegistroNaoEncontradoException("Hóspede", hospedeId);
        var quarto = await _quartos.ObterAsync(quartoId) ?? throw new RegistroNaoEncontradoException("Quarto", quartoId);

        // O construtor da Reserva valida datas e capacidade.
        var reserva = new Reserva(hospede, quarto, entrada, saida, quantidadeHospedes);

        var reservas = await _reservas.ObterTodosAsync();
        if (reservas.Any(r => r.Quarto.Id == quarto.Id && r.ConflitaCom(entrada, saida)))
            throw new QuartoIndisponivelException(quarto.Numero, entrada, saida);

        return await _reservas.AdicionarAsync(reserva);
    }

    public async Task<IReadOnlyList<Quarto>> ListarDisponiveisAsync(DateOnly entrada, DateOnly saida, int quantidadeHospedes)
    {
        var ocupados = (await _reservas.ObterTodosAsync())
            .Where(r => r.ConflitaCom(entrada, saida))
            .Select(r => r.Quarto.Id)
            .ToHashSet();

        return (await _quartos.ObterTodosAsync())
            .Where(q => !ocupados.Contains(q.Id) && q.Comporta(quantidadeHospedes))
            .OrderBy(q => q.Numero)
            .ToList();
    }

    public Task<Reserva> FazerCheckInAsync(int reservaId) => AlterarAsync(reservaId, r => r.FazerCheckIn());

    public Task<Reserva> FazerCheckOutAsync(int reservaId) => AlterarAsync(reservaId, r => r.FazerCheckOut());

    public Task<Reserva> CancelarAsync(int reservaId) => AlterarAsync(reservaId, r => r.Cancelar());

    public async Task<RelatorioHotel> GerarRelatorioAsync()
    {
        var reservas = await _reservas.ObterTodosAsync();
        var validas = reservas.Where(r => r.Status != EStatusReserva.Cancelada).ToList();

        ETipoQuarto? maisReservado = validas.Count == 0
            ? null
            : validas.GroupBy(r => r.Quarto.Tipo).OrderByDescending(g => g.Count()).First().Key;

        return new RelatorioHotel(
            ReservasAtivas: reservas.Count(r => r.Ativa),
            ReservasFinalizadas: reservas.Count(r => r.Status == EStatusReserva.Finalizada),
            ReservasCanceladas: reservas.Count(r => r.Status == EStatusReserva.Cancelada),
            FaturamentoRealizado: reservas.Where(r => r.Status == EStatusReserva.Finalizada).Sum(r => r.ValorTotal),
            FaturamentoPrevisto: reservas.Where(r => r.Ativa).Sum(r => r.ValorTotal),
            TipoMaisReservado: maisReservado);
    }

    private async Task<Reserva> AlterarAsync(int reservaId, Action<Reserva> alteracao)
    {
        var reserva = await _reservas.ObterAsync(reservaId) ?? throw new RegistroNaoEncontradoException("Reserva", reservaId);
        alteracao(reserva);
        return await _reservas.AtualizarAsync(reserva);
    }
}
