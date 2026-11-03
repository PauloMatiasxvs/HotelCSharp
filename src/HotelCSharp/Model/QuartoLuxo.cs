using HotelCSharp.Enums;

namespace HotelCSharp.Model;

// Não é sealed: a Suíte herda daqui (Quarto -> QuartoLuxo -> Suite).
public class QuartoLuxo : Quarto
{
    public const decimal AdicionalPorPessoaExtra = 60m;

    public QuartoLuxo(int numero, int andar) : base(numero, andar) { }

    public override ETipoQuarto Tipo => ETipoQuarto.Luxo;
    public override int CapacidadeMaxima => 3;
    protected override decimal ValorDiariaBase => 320m;

    // SOBRESCRITA: reaproveita a regra da superclasse com base.CalcularDiaria
    // e cobra cada pessoa acima de duas.
    public override decimal CalcularDiaria(int hospedes) =>
        base.CalcularDiaria(hospedes) + Math.Max(0, hospedes - 2) * AdicionalPorPessoaExtra;
}
