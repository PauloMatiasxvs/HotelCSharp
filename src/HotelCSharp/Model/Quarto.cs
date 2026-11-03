using HotelCSharp.Enums;

namespace HotelCSharp.Model;

// SUPERCLASSE abstrata: não existe "quarto sem tipo", só Standard, Luxo e Suíte.
public abstract class Quarto : EntidadeBase
{
    public const decimal AcrescimoFimDeSemana = 0.15m;

    public int Numero { get; }
    public int Andar { get; }

    // abstract: cada filha é obrigada a informar.
    public abstract ETipoQuarto Tipo { get; }
    public abstract int CapacidadeMaxima { get; }
    protected abstract decimal ValorDiariaBase { get; }

    // protected: só as filhas chamam, via : base(numero, andar).
    protected Quarto(int numero, int andar)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(numero);
        ArgumentOutOfRangeException.ThrowIfNegative(andar);

        Numero = numero;
        Andar = andar;
    }

    // virtual: a filha PODE ter outra regra de preço (o Luxo cobra pessoa extra).
    public virtual decimal CalcularDiaria(int hospedes)
    {
        ValidarHospedes(hospedes);
        return ValorDiariaBase;
    }

    // SOBRECARGA: mesmo nome, parâmetros diferentes. Chama a versão de cima,
    // que é polimórfica, e acrescenta o valor de fim de semana.
    public decimal CalcularDiaria(int hospedes, DateOnly data)
    {
        var diaria = CalcularDiaria(hospedes);
        return EhFimDeSemana(data) ? Math.Round(diaria * (1 + AcrescimoFimDeSemana), 2) : diaria;
    }

    public bool Comporta(int hospedes) => hospedes >= 1 && hospedes <= CapacidadeMaxima;

    protected void ValidarHospedes(int hospedes)
    {
        if (!Comporta(hospedes))
            throw new ArgumentOutOfRangeException(nameof(hospedes), hospedes,
                $"O quarto {Numero} aceita de 1 a {CapacidadeMaxima} hóspedes.");
    }

    // Noite de sexta e de sábado.
    private static bool EhFimDeSemana(DateOnly data) =>
        data.DayOfWeek is DayOfWeek.Friday or DayOfWeek.Saturday;

    public override string ToString() =>
        $"Quarto {Numero} ({Tipo}, {Andar}º andar, até {CapacidadeMaxima} pessoas)";
}
