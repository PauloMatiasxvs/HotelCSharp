using HotelCSharp.Enums;
using HotelCSharp.Exceptions;

namespace HotelCSharp.Model;

public class Reserva : EntidadeBase
{
    // Composição: a reserva TEM um hóspede e TEM um quarto.
    public Hospede Hospede { get; }
    public Quarto Quarto { get; }
    public DateOnly Entrada { get; }
    public DateOnly Saida { get; }
    public int QuantidadeHospedes { get; }

    // Encapsulamento: o status só muda pelos métodos de check-in, check-out e cancelamento.
    public EStatusReserva Status { get; private set; }

    public int Noites => Saida.DayNumber - Entrada.DayNumber;

    public bool Ativa => Status is EStatusReserva.Confirmada or EStatusReserva.CheckIn;

    // Propriedade calculada: soma noite a noite, porque sexta e sábado custam mais.
    // Quarto.CalcularDiaria é polimórfico: cada tipo de quarto aplica a sua regra.
    public decimal ValorTotal =>
        Enumerable.Range(0, Noites).Sum(noite => Quarto.CalcularDiaria(QuantidadeHospedes, Entrada.AddDays(noite)));

    public Reserva(Hospede hospede, Quarto quarto, DateOnly entrada, DateOnly saida, int quantidadeHospedes)
        : this(hospede, quarto, entrada, saida, quantidadeHospedes, EStatusReserva.Confirmada) { }

    // Usado pelos repositórios para remontar uma reserva salva com qualquer status.
    internal Reserva(Hospede hospede, Quarto quarto, DateOnly entrada, DateOnly saida,
        int quantidadeHospedes, EStatusReserva status)
    {
        ArgumentNullException.ThrowIfNull(hospede);
        ArgumentNullException.ThrowIfNull(quarto);

        if (saida <= entrada)
            throw new ReservaInvalidaException("A data de saída precisa ser depois da entrada.");
        if (!quarto.Comporta(quantidadeHospedes))
            throw new ReservaInvalidaException(
                $"O quarto {quarto.Numero} ({quarto.Tipo}) aceita até {quarto.CapacidadeMaxima} hóspedes, e foram pedidos {quantidadeHospedes}.");

        Hospede = hospede;
        Quarto = quarto;
        Entrada = entrada;
        Saida = saida;
        QuantidadeHospedes = quantidadeHospedes;
        Status = status;
    }

    public bool ConflitaCom(DateOnly entrada, DateOnly saida) => Ativa && entrada < Saida && saida > Entrada;

    public void FazerCheckIn()
    {
        GarantirStatus(EStatusReserva.Confirmada, "fazer check-in");
        Status = EStatusReserva.CheckIn;
    }

    public void FazerCheckOut()
    {
        GarantirStatus(EStatusReserva.CheckIn, "fazer check-out");
        Status = EStatusReserva.Finalizada;
    }

    public void Cancelar()
    {
        GarantirStatus(EStatusReserva.Confirmada, "cancelar");
        Status = EStatusReserva.Cancelada;
    }

    private void GarantirStatus(EStatusReserva esperado, string acao)
    {
        if (Status != esperado)
            throw new ReservaInvalidaException($"Não é possível {acao}: a reserva #{Id} está {Status}, e precisa estar {esperado}.");
    }

    public override string ToString() =>
        $"Reserva #{Id}: {Hospede.Nome}, quarto {Quarto.Numero} ({Quarto.Tipo}), " +
        $"{Entrada:dd/MM} a {Saida:dd/MM} ({Noites} noites, {QuantidadeHospedes} pessoas), R$ {ValorTotal:N2} [{Status}]";
}
