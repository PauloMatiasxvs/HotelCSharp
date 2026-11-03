using HotelCSharp.Enums;

namespace HotelCSharp.Model;

// Factory: decide qual subclasse criar a partir do tipo (útil quando o tipo vem do banco).
public static class QuartoFactory
{
    public static Quarto Criar(ETipoQuarto tipo, int numero, int andar) => tipo switch
    {
        ETipoQuarto.Standard => new QuartoStandard(numero, andar),
        ETipoQuarto.Luxo => new QuartoLuxo(numero, andar),
        ETipoQuarto.Suite => new Suite(numero, andar),
        _ => throw new ArgumentOutOfRangeException(nameof(tipo), tipo, "Tipo de quarto desconhecido.")
    };
}
