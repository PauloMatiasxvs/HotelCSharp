namespace HotelCSharp.Exceptions;

// Superclasse das regras do hotel que foram violadas. Herda de Exception (do .NET)
// e permite tratar todas as filhas com um único catch (HotelException).
public abstract class HotelException : Exception
{
    protected HotelException(string mensagem) : base(mensagem) { }
}

public sealed class QuartoIndisponivelException : HotelException
{
    public QuartoIndisponivelException(int numeroQuarto, DateOnly entrada, DateOnly saida)
        : base($"O quarto {numeroQuarto} já está reservado entre {entrada:dd/MM/yyyy} e {saida:dd/MM/yyyy}.") { }
}

public sealed class ReservaInvalidaException : HotelException
{
    public ReservaInvalidaException(string mensagem) : base(mensagem) { }
}

public sealed class CadastroDuplicadoException : HotelException
{
    public CadastroDuplicadoException(string mensagem) : base(mensagem) { }
}

public sealed class RegistroNaoEncontradoException : HotelException
{
    public RegistroNaoEncontradoException(string entidade, int id)
        : base($"{entidade} com Id {id} não foi encontrado.") { }
}
