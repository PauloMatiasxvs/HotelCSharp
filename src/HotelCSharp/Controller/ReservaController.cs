using HotelCSharp.Model;
using HotelCSharp.Services;

namespace HotelCSharp.Controller;

// Reserva não é um cadastro simples: o controller só recebe o pedido e repassa ao service,
// onde estão as regras (disponibilidade, check-in, check-out, relatório).
public class ReservaController(ReservaService service)
{
    public Task<Reserva> ReservarAsync(int hospedeId, int quartoId, DateOnly entrada, DateOnly saida, int quantidadeHospedes) =>
        service.ReservarAsync(hospedeId, quartoId, entrada, saida, quantidadeHospedes);

    public Task<IReadOnlyList<Quarto>> ListarDisponiveisAsync(DateOnly entrada, DateOnly saida, int quantidadeHospedes) =>
        service.ListarDisponiveisAsync(entrada, saida, quantidadeHospedes);

    public Task<Reserva> FazerCheckInAsync(int reservaId) => service.FazerCheckInAsync(reservaId);

    public Task<Reserva> FazerCheckOutAsync(int reservaId) => service.FazerCheckOutAsync(reservaId);

    public Task<Reserva> CancelarAsync(int reservaId) => service.CancelarAsync(reservaId);

    public Task<RelatorioHotel> GerarRelatorioAsync() => service.GerarRelatorioAsync();
}
