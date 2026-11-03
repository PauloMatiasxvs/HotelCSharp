using HotelCSharp.Model;

namespace HotelCSharp.Controller.Interfaces;

/// <summary>
/// Porta de entrada para os cadastros (no curso: "simular uma API").
/// </summary>
/// <typeparam name="T">Entidade controlada.</typeparam>
public interface ICrudController<T> where T : EntidadeBase
{
    Task<T> AdicionarAsync(T modelo);
    Task<IReadOnlyList<T>> ObterTodosAsync();
    Task<T> ObterAsync(int id);
    Task<T> AtualizarAsync(int id, T modelo);
    Task DeletarAsync(int id);
}
