using HotelCSharp.Controller.Interfaces;
using HotelCSharp.Exceptions;
using HotelCSharp.Model;
using HotelCSharp.Repository.Interfaces;

namespace HotelCSharp.Controller;

// SUPERCLASSE dos controllers de cadastro. No curso cada controller repetia o mesmo código;
// aqui o comportamento padrão fica num lugar só, e os métodos são virtual para as filhas
// sobrescreverem só o que tem regra (ex.: não deixar cadastrar CPF repetido).
public abstract class CrudController<T> : ICrudController<T> where T : EntidadeBase
{
    protected ICrudRepository<T> Repositorio { get; }

    protected CrudController(ICrudRepository<T> repositorio)
    {
        ArgumentNullException.ThrowIfNull(repositorio);
        Repositorio = repositorio;
    }

    public virtual Task<T> AdicionarAsync(T modelo) => Repositorio.AdicionarAsync(modelo);

    public virtual Task<IReadOnlyList<T>> ObterTodosAsync() => Repositorio.ObterTodosAsync();

    public virtual async Task<T> ObterAsync(int id) =>
        await Repositorio.ObterAsync(id) ?? throw new RegistroNaoEncontradoException(typeof(T).Name, id);

    public virtual Task<T> AtualizarAsync(int id, T modelo)
    {
        ArgumentNullException.ThrowIfNull(modelo);
        modelo.Id = id;
        return Repositorio.AtualizarAsync(modelo);
    }

    public virtual Task DeletarAsync(int id) => Repositorio.DeletarAsync(id);
}
