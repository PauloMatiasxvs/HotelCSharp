using HotelCSharp.Enums;

namespace HotelCSharp.Model;

public class QuartoStandard : Quarto
{
    public QuartoStandard(int numero, int andar) : base(numero, andar) { }

    public override ETipoQuarto Tipo => ETipoQuarto.Standard;
    public override int CapacidadeMaxima => 2;
    protected override decimal ValorDiariaBase => 180m;

    // Não sobrescreve CalcularDiaria: usa a regra da superclasse.
}
