namespace HotelCSharp.Model;

public class Hospede : EntidadeBase
{
    public string Nome { get; }
    public string Cpf { get; }
    public string? Telefone { get; private set; }
    public string? Email { get; private set; }

    // Nome e CPF são obrigatórios: vão no construtor.
    // Telefone e e-mail são opcionais: parâmetros com valor padrão.
    public Hospede(string nome, string cpf, string? telefone = null, string? email = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nome);

        var digitos = new string((cpf ?? string.Empty).Where(char.IsDigit).ToArray());
        if (digitos.Length != 11)
            throw new ArgumentException("O CPF precisa ter 11 dígitos.", nameof(cpf));

        Nome = nome.Trim();
        Cpf = digitos;
        Telefone = telefone;
        Email = email;
    }

    public void AtualizarContato(string? telefone, string? email)
    {
        Telefone = telefone;
        Email = email;
    }

    // Sobrescritas de object: texto amigável e igualdade pelo CPF.
    public override string ToString() => $"{Nome} (CPF {Cpf[..3]}.***.***-{Cpf[9..]})";

    public override bool Equals(object? obj) => obj is Hospede outro && outro.Cpf == Cpf;

    public override int GetHashCode() => Cpf.GetHashCode();
}
