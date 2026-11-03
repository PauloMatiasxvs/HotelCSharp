using HotelCSharp.Exceptions;
using HotelCSharp.Model;
using HotelCSharp.Repository.Interfaces;

namespace HotelCSharp.Repository.Memoria;

// Simula o banco com uma lista, como no curso. Por ser genérico (<T>), uma classe só
// substitui os vários repositórios quase iguais da pizzaria.
public class MemoriaRepository<T> : ICrudRepository<T> where T : EntidadeBase
{
    private readonly List<T> _itens = [];
    private int _ultimoId;

    public Task<T> AdicionarAsync(T modelo)
    {
        ArgumentNullException.ThrowIfNull(modelo);

        modelo.Id = ++_ultimoId;
        _itens.Add(modelo);
        return Task.FromResult(modelo);
    }

    // Devolve uma cópia: quem chama não consegue apagar itens do "banco" por fora.
    public Task<IReadOnlyList<T>> ObterTodosAsync() =>
        Task.FromResult<IReadOnlyList<T>>(_itens.ToList());

    public Task<T?> ObterAsync(int id) =>
        Task.FromResult(_itens.FirstOrDefault(item => item.Id == id));

    public Task<T> AtualizarAsync(T modelo)
    {
        ArgumentNullException.ThrowIfNull(modelo);

        var indice = _itens.FindIndex(item => item.Id == modelo.Id);
        if (indice < 0)
            throw new RegistroNaoEncontradoException(typeof(T).Name, modelo.Id);

        _itens[indice] = modelo;
        return Task.FromResult(modelo);
    }

    public Task DeletarAsync(int id)
    {
        if (_itens.RemoveAll(item => item.Id == id) == 0)
            throw new RegistroNaoEncontradoException(typeof(T).Name, id);

        return Task.CompletedTask;
    }
}
