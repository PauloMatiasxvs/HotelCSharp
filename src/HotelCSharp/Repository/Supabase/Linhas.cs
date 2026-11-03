using HotelCSharp.Enums;
using HotelCSharp.Model;

namespace HotelCSharp.Repository.Supabase;

// Formato das linhas das tabelas. Ficam separadas das entidades para o Model
// não depender do banco (a entidade não sabe que existe Supabase).
// Id null = linha nova: o banco gera o Id e ele não vai no JSON.

public sealed record HospedeLinha(int? Id, string Nome, string Cpf, string? Telefone, string? Email)
{
    public static HospedeLinha De(Hospede hospede) =>
        new(null, hospede.Nome, hospede.Cpf, hospede.Telefone, hospede.Email);

    public Hospede ParaEntidade() =>
        new Hospede(Nome, Cpf, Telefone, Email) { Id = Id ?? 0 };
}

public sealed record QuartoLinha(int? Id, int Numero, int Andar, string Tipo)
{
    public static QuartoLinha De(Quarto quarto) =>
        new(null, quarto.Numero, quarto.Andar, Banco.Texto(quarto.Tipo));

    public Quarto ParaEntidade()
    {
        var quarto = QuartoFactory.Criar(Banco.Enum<ETipoQuarto>(Tipo), Numero, Andar);
        quarto.Id = Id ?? 0;
        return quarto;
    }
}

public sealed record ReservaLinha(
    int? Id,
    int HospedeId,
    int QuartoId,
    DateOnly Entrada,
    DateOnly Saida,
    int QuantidadeHospedes,
    string Status,
    decimal ValorTotal,
    HospedeLinha? Hospede = null,
    QuartoLinha? Quarto = null)
{
    public static ReservaLinha De(Reserva reserva) =>
        new(null, reserva.Hospede.Id, reserva.Quarto.Id, reserva.Entrada, reserva.Saida,
            reserva.QuantidadeHospedes, Banco.Texto(reserva.Status), reserva.ValorTotal);

    public Reserva ParaEntidade()
    {
        var hospede = (Hospede ?? throw new InvalidOperationException("A consulta não trouxe o hóspede da reserva.")).ParaEntidade();
        var quarto = (Quarto ?? throw new InvalidOperationException("A consulta não trouxe o quarto da reserva.")).ParaEntidade();

        return new Reserva(hospede, quarto, Entrada, Saida, QuantidadeHospedes, Banco.Enum<EStatusReserva>(Status))
        {
            Id = Id ?? 0
        };
    }
}

// Enums são gravados em maiúsculas no banco (STANDARD, CHECKIN...).
internal static class Banco
{
    public static string Texto<TEnum>(TEnum valor) where TEnum : struct, Enum =>
        valor.ToString().ToUpperInvariant();

    public static TEnum Enum<TEnum>(string texto) where TEnum : struct, Enum =>
        System.Enum.Parse<TEnum>(texto, ignoreCase: true);
}
