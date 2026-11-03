using System.Text.Json;
using HotelCSharp.Enums;
using HotelCSharp.Model;
using HotelCSharp.Repository.Supabase;

namespace HotelCSharp.Tests;

// Garante que o JSON enviado e recebido bate com as colunas do banco (database/schema.sql).
public class SupabaseMapeamentoTests
{
    [Fact]
    public void LinhaNova_UsaSnakeCase_ENaoEnviaId()
    {
        var hospede = new Hospede("Ana Souza", "52998224725") { Id = 1 };
        var quarto = new Suite(301, 3) { Id = 2 };
        var reserva = new Reserva(hospede, quarto, new DateOnly(2026, 11, 10), new DateOnly(2026, 11, 12), 2);

        var json = JsonSerializer.Serialize(ReservaLinha.De(reserva), SupabaseClient.Json);

        Assert.Contains("\"hospede_id\":1", json);
        Assert.Contains("\"quarto_id\":2", json);
        Assert.Contains("\"quantidade_hospedes\":2", json);
        Assert.Contains("\"status\":\"CONFIRMADA\"", json);
        Assert.Contains("\"entrada\":\"2026-11-10\"", json);
        Assert.DoesNotContain("\"id\"", json);
        Assert.DoesNotContain("\"hospede\":", json);
    }

    [Fact]
    public void LinhaDoBanco_ViraAEntidadeCerta_ComHospedeEQuarto()
    {
        const string json = """
            {"id":7,"hospede_id":1,"quarto_id":2,"entrada":"2026-11-10","saida":"2026-11-12",
             "quantidade_hospedes":2,"status":"CHECKIN","valor_total":1210.00,"criado_em":"2026-11-03T16:30:00+00:00",
             "hospede":{"id":1,"nome":"Ana Souza","cpf":"52998224725","telefone":null,"email":null,"criado_em":"2026-11-03T16:00:00+00:00"},
             "quarto":{"id":2,"numero":301,"andar":3,"tipo":"SUITE"}}
            """;

        var reserva = JsonSerializer.Deserialize<ReservaLinha>(json, SupabaseClient.Json)!.ParaEntidade();

        Assert.Equal(7, reserva.Id);
        Assert.Equal(EStatusReserva.CheckIn, reserva.Status);
        Assert.Equal(1, reserva.Hospede.Id);
        Assert.IsType<Suite>(reserva.Quarto);
        Assert.Equal(2, reserva.Quarto.Id);
        Assert.Equal(1210m, reserva.ValorTotal);
    }
}
