using HotelCSharp.Exceptions;
using HotelCSharp.Model;
using HotelCSharp.Repository.Interfaces;

namespace HotelCSharp.Repository.Supabase;

// SUPERCLASSE dos repositórios do Supabase (Template Method): o CRUD é igual para
// todas as tabelas, então fica aqui. Cada filha só sobrescreve o nome da tabela,
// as colunas da consulta e a conversão linha <-> entidade.
public abstract class SupabaseRepository<TEntidade, TLinha> : ICrudRepository<TEntidade>
    where TEntidade : EntidadeBase
{
    protected SupabaseClient Cliente { get; }

    protected SupabaseRepository(SupabaseClient cliente)
    {
        ArgumentNullException.ThrowIfNull(cliente);
        Cliente = cliente;
    }

    protected abstract string Tabela { get; }

    // virtual: a reserva sobrescreve para trazer hóspede e quarto juntos.
    protected virtual string Selecao => "*";

    protected abstract TLinha ParaLinha(TEntidade entidade);
    protected abstract TEntidade ParaEntidade(TLinha linha);

    public async Task<TEntidade> AdicionarAsync(TEntidade modelo)
    {
        ArgumentNullException.ThrowIfNull(modelo);

        var salva = await Cliente.InserirAsync(Tabela, ParaLinha(modelo), Selecao);
        modelo.Id = ParaEntidade(salva).Id;
        return modelo;
    }

    public async Task<IReadOnlyList<TEntidade>> ObterTodosAsync()
    {
        var linhas = await Cliente.SelecionarAsync<TLinha>(Tabela, $"select={Selecao}&order=id.asc");
        return linhas.Select(ParaEntidade).ToList();
    }

    public async Task<TEntidade?> ObterAsync(int id)
    {
        var linhas = await Cliente.SelecionarAsync<TLinha>(Tabela, $"select={Selecao}&id=eq.{id}");
        return linhas.Count == 0 ? null : ParaEntidade(linhas[0]);
    }

    public async Task<TEntidade> AtualizarAsync(TEntidade modelo)
    {
        ArgumentNullException.ThrowIfNull(modelo);

        var atualizada = await Cliente.AtualizarAsync(Tabela, modelo.Id, ParaLinha(modelo), Selecao);
        if (atualizada is null)
            throw new RegistroNaoEncontradoException(typeof(TEntidade).Name, modelo.Id);

        return modelo;
    }

    public async Task DeletarAsync(int id)
    {
        if (await Cliente.DeletarAsync(Tabela, id) == 0)
            throw new RegistroNaoEncontradoException(typeof(TEntidade).Name, id);
    }
}

public sealed class HospedeSupabaseRepository(SupabaseClient cliente)
    : SupabaseRepository<Hospede, HospedeLinha>(cliente)
{
    protected override string Tabela => "hospedes";
    protected override HospedeLinha ParaLinha(Hospede entidade) => HospedeLinha.De(entidade);
    protected override Hospede ParaEntidade(HospedeLinha linha) => linha.ParaEntidade();
}

public sealed class QuartoSupabaseRepository(SupabaseClient cliente)
    : SupabaseRepository<Quarto, QuartoLinha>(cliente)
{
    protected override string Tabela => "quartos";
    protected override QuartoLinha ParaLinha(Quarto entidade) => QuartoLinha.De(entidade);
    protected override Quarto ParaEntidade(QuartoLinha linha) => linha.ParaEntidade();
}

public sealed class ReservaSupabaseRepository(SupabaseClient cliente)
    : SupabaseRepository<Reserva, ReservaLinha>(cliente)
{
    protected override string Tabela => "reservas";

    // Embedding do PostgREST: traz o hóspede e o quarto pela chave estrangeira.
    protected override string Selecao => "*,hospede:hospedes(*),quarto:quartos(*)";

    protected override ReservaLinha ParaLinha(Reserva entidade) => ReservaLinha.De(entidade);
    protected override Reserva ParaEntidade(ReservaLinha linha) => linha.ParaEntidade();
}
