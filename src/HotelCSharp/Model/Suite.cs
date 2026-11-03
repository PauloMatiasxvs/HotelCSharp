using HotelCSharp.Enums;

namespace HotelCSharp.Model;

// sealed: ninguém herda da suíte.
public sealed class Suite : QuartoLuxo
{
    public const decimal TaxaServico = 0.10m;

    public Suite(int numero, int andar) : base(numero, andar) { }

    public override ETipoQuarto Tipo => ETipoQuarto.Suite;
    public override int CapacidadeMaxima => 4;
    protected override decimal ValorDiariaBase => 550m;

    // Aqui "base" é o QuartoLuxo, que por sua vez chama o Quarto:
    // diária base + pessoa extra (regra do Luxo) + 10% de serviço (regra da Suíte).
    public override decimal CalcularDiaria(int hospedes) =>
        Math.Round(base.CalcularDiaria(hospedes) * (1 + TaxaServico), 2);
}
