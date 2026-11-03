using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HotelCSharp.Repository.Supabase;

// Cliente mínimo da API REST do Supabase (PostgREST): cada tabela vira o endpoint /rest/v1/<tabela>.
public sealed class SupabaseClient
{
    public const string VariavelUrl = "SUPABASE_URL";
    public const string VariavelChave = "SUPABASE_KEY";
    public const string ArquivoLocal = "supabase.local.json";

    // Colunas do banco em snake_case (hospede_id) <-> propriedades em PascalCase (HospedeId).
    internal static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly HttpClient _http;

    public SupabaseClient(string url, string chave, HttpClient? http = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(url);
        ArgumentException.ThrowIfNullOrWhiteSpace(chave);

        _http = http ?? new HttpClient();
        _http.BaseAddress = new Uri(url.TrimEnd('/') + "/rest/v1/");
        _http.DefaultRequestHeaders.Add("apikey", chave);

        // Chave anon antiga (JWT) também precisa ir no Authorization; a publishable nova não.
        if (chave.StartsWith("eyJ", StringComparison.Ordinal))
            _http.DefaultRequestHeaders.Authorization = new("Bearer", chave);
    }

    // Lê URL e chave das variáveis de ambiente ou do arquivo supabase.local.json.
    // Sem nenhum dos dois, o programa usa o banco em memória.
    public static SupabaseClient? CriarDoAmbiente()
    {
        var url = Environment.GetEnvironmentVariable(VariavelUrl);
        var chave = Environment.GetEnvironmentVariable(VariavelChave);

        if (string.IsNullOrWhiteSpace(url) || string.IsNullOrWhiteSpace(chave))
            (url, chave) = LerArquivoLocal();

        return string.IsNullOrWhiteSpace(url) || string.IsNullOrWhiteSpace(chave)
            ? null
            : new SupabaseClient(url, chave);
    }

    // Procura o arquivo da pasta atual para cima. Ele fica fora do Git (.gitignore).
    private static (string? Url, string? Chave) LerArquivoLocal()
    {
        for (var pasta = new DirectoryInfo(Directory.GetCurrentDirectory()); pasta is not null; pasta = pasta.Parent)
        {
            var caminho = Path.Combine(pasta.FullName, ArquivoLocal);
            if (!File.Exists(caminho))
                continue;

            using var json = JsonDocument.Parse(File.ReadAllText(caminho));
            return (Ler(json, "url"), Ler(json, "key"));
        }

        return (null, null);

        static string? Ler(JsonDocument json, string nome) =>
            json.RootElement.TryGetProperty(nome, out var valor) ? valor.GetString() : null;
    }

    public async Task<List<T>> SelecionarAsync<T>(string tabela, string consulta)
    {
        using var resposta = await _http.GetAsync($"{tabela}?{consulta}");
        return await LerListaAsync<T>(resposta);
    }

    public async Task<T> InserirAsync<T>(string tabela, T linha, string selecao)
    {
        using var requisicao = new HttpRequestMessage(HttpMethod.Post, $"{tabela}?select={selecao}")
        {
            Content = JsonContent.Create(linha, options: Json)
        };
        requisicao.Headers.Add("Prefer", "return=representation");

        using var resposta = await _http.SendAsync(requisicao);
        var linhas = await LerListaAsync<T>(resposta);
        return linhas.Single();
    }

    public async Task<T?> AtualizarAsync<T>(string tabela, int id, T linha, string selecao)
    {
        using var requisicao = new HttpRequestMessage(HttpMethod.Patch, $"{tabela}?id=eq.{id}&select={selecao}")
        {
            Content = JsonContent.Create(linha, options: Json)
        };
        requisicao.Headers.Add("Prefer", "return=representation");

        using var resposta = await _http.SendAsync(requisicao);
        var linhas = await LerListaAsync<T>(resposta);
        return linhas.SingleOrDefault();
    }

    // Devolve quantas linhas foram apagadas.
    public async Task<int> DeletarAsync(string tabela, int id)
    {
        using var requisicao = new HttpRequestMessage(HttpMethod.Delete, $"{tabela}?id=eq.{id}&select=id");
        requisicao.Headers.Add("Prefer", "return=representation");

        using var resposta = await _http.SendAsync(requisicao);
        var linhas = await LerListaAsync<JsonElement>(resposta);
        return linhas.Count;
    }

    private static async Task<List<T>> LerListaAsync<T>(HttpResponseMessage resposta)
    {
        if (!resposta.IsSuccessStatusCode)
        {
            var corpo = await resposta.Content.ReadAsStringAsync();
            throw new HttpRequestException(
                $"Supabase respondeu {(int)resposta.StatusCode} ({resposta.ReasonPhrase}): {corpo}",
                null, resposta.StatusCode);
        }

        return await resposta.Content.ReadFromJsonAsync<List<T>>(Json) ?? [];
    }
}
