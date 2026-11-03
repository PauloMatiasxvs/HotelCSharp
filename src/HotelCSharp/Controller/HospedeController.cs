using HotelCSharp.Exceptions;
using HotelCSharp.Model;
using HotelCSharp.Repository.Interfaces;

namespace HotelCSharp.Controller;

public class HospedeController(ICrudRepository<Hospede> repositorio) : CrudController<Hospede>(repositorio)
{
    // SOBRESCRITA: acrescenta a regra do CPF único e depois usa a versão da superclasse.
    public override async Task<Hospede> AdicionarAsync(Hospede modelo)
    {
        ArgumentNullException.ThrowIfNull(modelo);

        if (await BuscarPorCpfAsync(modelo.Cpf) is not null)
            throw new CadastroDuplicadoException($"Já existe hóspede com o CPF terminado em {modelo.Cpf[9..]}.");

        return await base.AdicionarAsync(modelo);
    }

    public async Task<Hospede?> BuscarPorCpfAsync(string cpf)
    {
        var digitos = new string(cpf.Where(char.IsDigit).ToArray());
        return (await ObterTodosAsync()).FirstOrDefault(h => h.Cpf == digitos);
    }

    public async Task<Hospede> ObterOuCadastrarAsync(Hospede hospede) =>
        await BuscarPorCpfAsync(hospede.Cpf) ?? await AdicionarAsync(hospede);
}
