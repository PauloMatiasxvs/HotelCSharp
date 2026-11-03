using HotelCSharp.Enums;
using HotelCSharp.Exceptions;
using HotelCSharp.Model;
using HotelCSharp.Repository.Memoria;
using HotelCSharp.Services;

namespace HotelCSharp.Tests;

public class ReservaServiceTests
{
    private static readonly DateOnly Terca = new(2026, 11, 10);

    private readonly MemoriaRepository<Hospede> _hospedes = new();
    private readonly MemoriaRepository<Quarto> _quartos = new();
    private readonly MemoriaRepository<Reserva> _reservas = new();
    private readonly ReservaService _service;

    public ReservaServiceTests() => _service = new ReservaService(_reservas, _quartos, _hospedes);

    private Task<Hospede> CadastrarHospedeAsync() => _hospedes.AdicionarAsync(new Hospede("Ana Souza", "52998224725"));

    [Fact]
    public async Task Reservar_DatasQueSeCruzam_LancaQuartoIndisponivel()
    {
        var hospede = await CadastrarHospedeAsync();
        var suite = await _quartos.AdicionarAsync(new Suite(301, 3));
        await _service.ReservarAsync(hospede.Id, suite.Id, Terca, Terca.AddDays(3), 2);

        await Assert.ThrowsAsync<QuartoIndisponivelException>(() =>
            _service.ReservarAsync(hospede.Id, suite.Id, Terca.AddDays(1), Terca.AddDays(2), 2));
    }

    [Fact]
    public async Task Reservar_DepoisDeCancelar_LiberaOQuarto()
    {
        var hospede = await CadastrarHospedeAsync();
        var suite = await _quartos.AdicionarAsync(new Suite(301, 3));
        var primeira = await _service.ReservarAsync(hospede.Id, suite.Id, Terca, Terca.AddDays(3), 2);

        await _service.CancelarAsync(primeira.Id);
        var segunda = await _service.ReservarAsync(hospede.Id, suite.Id, Terca, Terca.AddDays(3), 2);

        Assert.Equal(EStatusReserva.Confirmada, segunda.Status);
    }

    [Fact]
    public async Task Reservar_HospedeInexistente_LancaRegistroNaoEncontrado()
    {
        var suite = await _quartos.AdicionarAsync(new Suite(301, 3));

        await Assert.ThrowsAsync<RegistroNaoEncontradoException>(() =>
            _service.ReservarAsync(99, suite.Id, Terca, Terca.AddDays(1), 2));
    }

    [Fact]
    public async Task ListarDisponiveis_TiraOcupadosEQuemNaoComporta()
    {
        var hospede = await CadastrarHospedeAsync();
        await _quartos.AdicionarAsync(new QuartoStandard(101, 1));   // não comporta 3
        await _quartos.AdicionarAsync(new QuartoLuxo(201, 2));       // livre
        var suite = await _quartos.AdicionarAsync(new Suite(301, 3));
        await _service.ReservarAsync(hospede.Id, suite.Id, Terca, Terca.AddDays(2), 2);

        var livres = await _service.ListarDisponiveisAsync(Terca, Terca.AddDays(1), 3);

        Assert.Equal([201], livres.Select(q => q.Numero));
    }

    [Fact]
    public async Task Relatorio_SeparaFaturamentoPorStatus()
    {
        var hospede = await CadastrarHospedeAsync();
        var standard = await _quartos.AdicionarAsync(new QuartoStandard(101, 1));
        var suite = await _quartos.AdicionarAsync(new Suite(301, 3));
        var luxo = await _quartos.AdicionarAsync(new QuartoLuxo(201, 2));

        var finalizada = await _service.ReservarAsync(hospede.Id, standard.Id, Terca, Terca.AddDays(2), 2);   // 2 x 180
        await _service.FazerCheckInAsync(finalizada.Id);
        await _service.FazerCheckOutAsync(finalizada.Id);
        await _service.ReservarAsync(hospede.Id, suite.Id, Terca, Terca.AddDays(1), 2);                       // 605
        var cancelada = await _service.ReservarAsync(hospede.Id, luxo.Id, Terca, Terca.AddDays(1), 2);
        await _service.CancelarAsync(cancelada.Id);

        var relatorio = await _service.GerarRelatorioAsync();

        Assert.Equal(1, relatorio.ReservasAtivas);
        Assert.Equal(1, relatorio.ReservasFinalizadas);
        Assert.Equal(1, relatorio.ReservasCanceladas);
        Assert.Equal(360m, relatorio.FaturamentoRealizado);
        Assert.Equal(605m, relatorio.FaturamentoPrevisto);
    }
}
