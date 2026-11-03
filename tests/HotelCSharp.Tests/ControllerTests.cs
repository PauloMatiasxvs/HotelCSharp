using HotelCSharp.Controller;
using HotelCSharp.Exceptions;
using HotelCSharp.Model;
using HotelCSharp.Repository.Memoria;

namespace HotelCSharp.Tests;

public class ControllerTests
{
    [Fact]
    public async Task Hospede_CpfRepetido_LancaCadastroDuplicado()
    {
        var controller = new HospedeController(new MemoriaRepository<Hospede>());
        await controller.AdicionarAsync(new Hospede("Ana Souza", "529.982.247-25"));

        // mesmo CPF, escrito sem máscara
        await Assert.ThrowsAsync<CadastroDuplicadoException>(() =>
            controller.AdicionarAsync(new Hospede("Ana S.", "52998224725")));
    }

    [Fact]
    public async Task Hospede_ObterOuCadastrar_DevolveOQueJaExiste()
    {
        var controller = new HospedeController(new MemoriaRepository<Hospede>());
        var primeiro = await controller.ObterOuCadastrarAsync(new Hospede("Ana Souza", "52998224725"));

        var segundo = await controller.ObterOuCadastrarAsync(new Hospede("Ana Souza", "52998224725"));

        Assert.Equal(primeiro.Id, segundo.Id);
        Assert.Single(await controller.ObterTodosAsync());
    }

    [Fact]
    public async Task Quarto_NumeroRepetido_LancaCadastroDuplicado()
    {
        var controller = new QuartoController(new MemoriaRepository<Quarto>());
        await controller.AdicionarAsync(new QuartoStandard(101, 1));

        await Assert.ThrowsAsync<CadastroDuplicadoException>(() =>
            controller.AdicionarAsync(new Suite(101, 1)));
    }

    [Fact]
    public async Task Obter_Inexistente_LancaRegistroNaoEncontrado()
    {
        var controller = new QuartoController(new MemoriaRepository<Quarto>());

        await Assert.ThrowsAsync<RegistroNaoEncontradoException>(() => controller.ObterAsync(42));
    }
}
