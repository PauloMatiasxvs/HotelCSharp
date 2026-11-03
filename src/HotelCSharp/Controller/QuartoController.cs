using HotelCSharp.Exceptions;
using HotelCSharp.Model;
using HotelCSharp.Repository.Interfaces;

namespace HotelCSharp.Controller;

public class QuartoController(ICrudRepository<Quarto> repositorio) : CrudController<Quarto>(repositorio)
{
    // SOBRESCRITA: não deixa cadastrar dois quartos com o mesmo número.
    public override async Task<Quarto> AdicionarAsync(Quarto modelo)
    {
        ArgumentNullException.ThrowIfNull(modelo);

        if ((await ObterTodosAsync()).Any(q => q.Numero == modelo.Numero))
            throw new CadastroDuplicadoException($"O quarto {modelo.Numero} já está cadastrado.");

        return await base.AdicionarAsync(modelo);
    }
}
