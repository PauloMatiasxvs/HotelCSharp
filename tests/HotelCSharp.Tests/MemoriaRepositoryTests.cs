using HotelCSharp.Exceptions;
using HotelCSharp.Model;
using HotelCSharp.Repository.Memoria;

namespace HotelCSharp.Tests;

public class MemoriaRepositoryTests
{
    private readonly MemoriaRepository<Quarto> _repositorio = new();

    [Fact]
    public async Task Adicionar_GeraIdsEmSequencia()
    {
        var primeiro = await _repositorio.AdicionarAsync(new QuartoStandard(101, 1));
        var segundo = await _repositorio.AdicionarAsync(new QuartoLuxo(201, 2));

        Assert.Equal(1, primeiro.Id);
        Assert.Equal(2, segundo.Id);
    }

    [Fact]
    public async Task ObterTodos_DevolveUmaFotoQueNaoMudaDepois()
    {
        await _repositorio.AdicionarAsync(new QuartoStandard(101, 1));
        var antes = await _repositorio.ObterTodosAsync();

        await _repositorio.AdicionarAsync(new QuartoLuxo(201, 2));

        Assert.Single(antes);
    }

    [Fact]
    public async Task Atualizar_Inexistente_Lanca()
    {
        var quarto = new QuartoStandard(101, 1) { Id = 99 };

        await Assert.ThrowsAsync<RegistroNaoEncontradoException>(() => _repositorio.AtualizarAsync(quarto));
    }

    [Fact]
    public async Task Deletar_RemoveEDepoisNaoAcha()
    {
        var quarto = await _repositorio.AdicionarAsync(new QuartoStandard(101, 1));

        await _repositorio.DeletarAsync(quarto.Id);

        Assert.Null(await _repositorio.ObterAsync(quarto.Id));
        await Assert.ThrowsAsync<RegistroNaoEncontradoException>(() => _repositorio.DeletarAsync(quarto.Id));
    }
}
