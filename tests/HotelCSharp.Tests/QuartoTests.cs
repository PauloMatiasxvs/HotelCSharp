using HotelCSharp.Enums;
using HotelCSharp.Model;

namespace HotelCSharp.Tests;

public class QuartoTests
{
    private static readonly DateOnly Terca = new(2026, 11, 3);
    private static readonly DateOnly Sabado = new(2026, 11, 7);

    [Theory]
    [InlineData(ETipoQuarto.Standard, 2, 180)]
    [InlineData(ETipoQuarto.Luxo, 2, 320)]
    [InlineData(ETipoQuarto.Luxo, 3, 380)]   // pessoa extra (sobrescrita no Luxo)
    [InlineData(ETipoQuarto.Suite, 2, 605)]  // 550 + 10% de serviço
    [InlineData(ETipoQuarto.Suite, 4, 737)]  // (550 + 2 x 60) + 10%: regra do Luxo + regra da Suíte
    public void CalcularDiaria_AplicaARegraDeCadaTipo(ETipoQuarto tipo, int hospedes, double esperado)
    {
        var quarto = QuartoFactory.Criar(tipo, 100, 1);

        Assert.Equal((decimal)esperado, quarto.CalcularDiaria(hospedes));
    }

    [Fact]
    public void Polimorfismo_MesmaChamadaRodaAVersaoDoTipoReal()
    {
        List<Quarto> quartos = [new QuartoStandard(101, 1), new QuartoLuxo(201, 2), new Suite(301, 3)];

        var diarias = quartos.Select(q => q.CalcularDiaria(2));

        Assert.Equal([180m, 320m, 605m], diarias);
    }

    [Fact]
    public void Sobrecarga_ComData_CobraAcrescimoNoFimDeSemana()
    {
        var quarto = new QuartoStandard(101, 1);

        Assert.Equal(180m, quarto.CalcularDiaria(2, Terca));
        Assert.Equal(207m, quarto.CalcularDiaria(2, Sabado));
    }

    [Fact]
    public void CalcularDiaria_AcimaDaCapacidade_Lanca()
    {
        var quarto = new QuartoStandard(101, 1);

        Assert.Throws<ArgumentOutOfRangeException>(() => quarto.CalcularDiaria(3));
    }

    [Fact]
    public void Suite_EhUmQuartoLuxo_EUmQuarto()
    {
        Quarto quarto = QuartoFactory.Criar(ETipoQuarto.Suite, 301, 3);

        Assert.IsType<Suite>(quarto);
        Assert.IsAssignableFrom<QuartoLuxo>(quarto);
        Assert.Equal(ETipoQuarto.Suite, quarto.Tipo);
        Assert.Equal(4, quarto.CapacidadeMaxima);
    }

    [Fact]
    public void Construtor_NumeroInvalido_Lanca()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new QuartoStandard(0, 1));
    }
}
