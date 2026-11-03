using HotelCSharp.Model;

namespace HotelCSharp.Repository.Interfaces;

/// <summary>
/// Contrato de acesso a dados, igual para qualquer entidade.
/// Quem usa não sabe se os dados estão em memória ou no Supabase.
/// </summary>
/// <typeparam name="T">Entidade persistida. Precisa herdar de EntidadeBase (ter Id).</typeparam>
public interface ICrudRepository<T> where T : EntidadeBase
{
    /// <summary>Adiciona e devolve o modelo já com o Id gerado.</summary>
    Task<T> AdicionarAsync(T modelo);

    /// <summary>Lista todos os registros, ordenados pelo Id.</summary>
    Task<IReadOnlyList<T>> ObterTodosAsync();

    /// <summary>Busca pelo Id. Devolve null quando não existe.</summary>
    Task<T?> ObterAsync(int id);

    /// <summary>Atualiza um registro existente.</summary>
    Task<T> AtualizarAsync(T modelo);

    /// <summary>Remove pelo Id.</summary>
    Task DeletarAsync(int id);
}
