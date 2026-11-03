using HotelCSharp.Enums;
using HotelCSharp.Exceptions;
using HotelCSharp.Model;

namespace HotelCSharp.Tests;

public class ReservaTests
{
    private static readonly Hospede Ana = new("Ana Souza", "529.982.247-25");
    private static readonly DateOnly Terca = new(2026, 11, 3);

    private static Reserva NovaReserva(Quarto? quarto = null, int noites = 2, int hospedes = 2) =>
        new(Ana, quarto ?? new QuartoStandard(101, 1), Terca, Terca.AddDays(noites), hospedes);

    [Fact]
    public void ValorTotal_SomaNoiteANoite_ComSextaMaisCara()
    {
        // terça, quarta, quinta e sexta
        var reserva = NovaReserva(noites: 4);

        Assert.Equal(4, reserva.Noites);
        Assert.Equal(180m * 3 + 207m, reserva.ValorTotal);
    }

    [Fact]
    public void SaidaAntesDaEntrada_Lanca()
    {
        Assert.Throws<ReservaInvalidaException>(() =>
            new Reserva(Ana, new QuartoStandard(101, 1), Terca, Terca, 2));
    }

    [Fact]
    public void HospedesAcimaDaCapacidade_Lanca()
    {
        Assert.Throws<ReservaInvalidaException>(() => NovaReserva(new QuartoLuxo(201, 2), hospedes: 4));
    }

    [Fact]
    public void CicloCompleto_ConfirmadaCheckInFinalizada()
    {
        var reserva = NovaReserva();
        Assert.Equal(EStatusReserva.Confirmada, reserva.Status);

        reserva.FazerCheckIn();
        Assert.Equal(EStatusReserva.CheckIn, reserva.Status);

        reserva.FazerCheckOut();
        Assert.Equal(EStatusReserva.Finalizada, reserva.Status);
        Assert.False(reserva.Ativa);
    }

    [Fact]
    public void CheckOutSemCheckIn_Lanca()
    {
        Assert.Throws<ReservaInvalidaException>(() => NovaReserva().FazerCheckOut());
    }

    [Fact]
    public void CancelarDepoisDoCheckIn_Lanca()
    {
        var reserva = NovaReserva();
        reserva.FazerCheckIn();

        Assert.Throws<ReservaInvalidaException>(reserva.Cancelar);
    }

    [Theory]
    [InlineData(1, 3, true)]    // cruza o fim da reserva
    [InlineData(2, 4, false)]   // entra no dia em que a outra sai
    [InlineData(-2, 0, false)]  // sai no dia em que a outra entra
    [InlineData(-1, 5, true)]   // cobre a reserva inteira
    public void ConflitaCom_DetectaDatasQueSeCruzam(int deslocamentoEntrada, int deslocamentoSaida, bool conflita)
    {
        var reserva = NovaReserva(noites: 2);

        Assert.Equal(conflita, reserva.ConflitaCom(Terca.AddDays(deslocamentoEntrada), Terca.AddDays(deslocamentoSaida)));
    }

    [Fact]
    public void ReservaCancelada_NaoConflita()
    {
        var reserva = NovaReserva();
        reserva.Cancelar();

        Assert.False(reserva.ConflitaCom(Terca, Terca.AddDays(1)));
    }
}
