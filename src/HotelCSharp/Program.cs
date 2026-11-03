using System.Text;
using HotelCSharp.Controller;
using HotelCSharp.Enums;
using HotelCSharp.Exceptions;
using HotelCSharp.Model;
using HotelCSharp.Repository.Interfaces;
using HotelCSharp.Repository.Memoria;
using HotelCSharp.Repository.Supabase;
using HotelCSharp.Services;

Console.OutputEncoding = Encoding.UTF8;

Console.WriteLine("***************************************");
Console.WriteLine("************* Hotel CSharp ************");
Console.WriteLine("***************************************");
Console.WriteLine();

// Onde os dados ficam: Supabase se houver URL e chave configuradas; senão memória, como no curso.
// O resto do programa não muda, porque só conhece a interface ICrudRepository<T>.
ICrudRepository<Hospede> repositorioHospedes;
ICrudRepository<Quarto> repositorioQuartos;
ICrudRepository<Reserva> repositorioReservas;

var supabase = SupabaseClient.CriarDoAmbiente();
if (supabase is null)
{
    repositorioHospedes = new MemoriaRepository<Hospede>();
    repositorioQuartos = new MemoriaRepository<Quarto>();
    repositorioReservas = new MemoriaRepository<Reserva>();
    Console.WriteLine("Banco de dados: memória (para usar o Supabase, crie o supabase.local.json; veja o README)");
}
else
{
    repositorioHospedes = new HospedeSupabaseRepository(supabase);
    repositorioQuartos = new QuartoSupabaseRepository(supabase);
    repositorioReservas = new ReservaSupabaseRepository(supabase);
    Console.WriteLine("Banco de dados: Supabase (PostgreSQL)");
}
Console.WriteLine();

var hospedes = new HospedeController(repositorioHospedes);
var quartos = new QuartoController(repositorioQuartos);
var reservas = new ReservaController(new ReservaService(repositorioReservas, repositorioQuartos, repositorioHospedes));

try
{
    // 1º Cadastrar os quartos (só na primeira execução)
    if ((await quartos.ObterTodosAsync()).Count == 0)
    {
        await quartos.AdicionarAsync(QuartoFactory.Criar(ETipoQuarto.Standard, 101, 1));
        await quartos.AdicionarAsync(QuartoFactory.Criar(ETipoQuarto.Standard, 102, 1));
        await quartos.AdicionarAsync(QuartoFactory.Criar(ETipoQuarto.Luxo, 201, 2));
        await quartos.AdicionarAsync(QuartoFactory.Criar(ETipoQuarto.Luxo, 202, 2));
        await quartos.AdicionarAsync(QuartoFactory.Criar(ETipoQuarto.Suite, 301, 3));
    }

    var todosOsQuartos = await quartos.ObterTodosAsync();
    var luxo = todosOsQuartos.First(q => q.Tipo == ETipoQuarto.Luxo);
    var suite = todosOsQuartos.First(q => q.Tipo == ETipoQuarto.Suite);

    // 2º Polimorfismo: a mesma chamada, uma regra de preço por tipo de quarto
    var terca = ProximoDia(DayOfWeek.Tuesday);
    var sabado = ProximoDia(DayOfWeek.Saturday);
    Console.WriteLine("Diária para 2 pessoas (dia de semana / sábado):");
    foreach (var quarto in todosOsQuartos)
        Console.WriteLine($"  {quarto}: R$ {quarto.CalcularDiaria(2, terca):N2} / R$ {quarto.CalcularDiaria(2, sabado):N2}");
    Console.WriteLine();

    // 3º Hóspedes
    var ana = await hospedes.ObterOuCadastrarAsync(new Hospede("Ana Souza", "529.982.247-25", "(21) 99999-0001", "ana@email.com"));
    var bruno = await hospedes.ObterOuCadastrarAsync(new Hospede("Bruno Lima", "111.444.777-35", "(21) 99999-0002"));
    Console.WriteLine($"Hóspedes: {ana} e {bruno}");
    Console.WriteLine();

    // 4º Reserva completa: reservar, check-in e check-out
    var hoje = DateOnly.FromDateTime(DateTime.Today);
    var entradaAna = hoje.AddDays(1);
    var saidaAna = hoje.AddDays(4);

    var livres = await reservas.ListarDisponiveisAsync(entradaAna, saidaAna, 2);
    Console.WriteLine($"Quartos livres de {entradaAna:dd/MM} a {saidaAna:dd/MM} para 2 pessoas: {string.Join(", ", livres.Select(q => q.Numero))}");

    var reservaAna = await reservas.ReservarAsync(ana.Id, livres.First(q => q.Tipo == ETipoQuarto.Standard).Id, entradaAna, saidaAna, 2);
    Console.WriteLine($"Criada:     {reservaAna}");
    reservaAna = await reservas.FazerCheckInAsync(reservaAna.Id);
    Console.WriteLine($"Check-in:   {reservaAna}");
    reservaAna = await reservas.FazerCheckOutAsync(reservaAna.Id);
    Console.WriteLine($"Check-out:  {reservaAna}");

    var reservaBruno = await reservas.ReservarAsync(bruno.Id, suite.Id, hoje.AddDays(7), hoje.AddDays(10), 4);
    Console.WriteLine($"Criada:     {reservaBruno}");
    Console.WriteLine();

    // 5º O que as regras recusam (cada uma é uma exceção filha de HotelException)
    Console.WriteLine("Tentativas recusadas pelas regras:");
    await Tentar(() => reservas.ReservarAsync(ana.Id, suite.Id, hoje.AddDays(8), hoje.AddDays(9), 2));
    await Tentar(() => reservas.ReservarAsync(ana.Id, luxo.Id, hoje.AddDays(20), hoje.AddDays(22), 4));
    await Tentar(() => reservas.FazerCheckOutAsync(reservaBruno.Id));
    await Tentar(() => hospedes.AdicionarAsync(new Hospede("Ana Cópia", "52998224725")));
    Console.WriteLine();

    // 6º Relatório
    var relatorio = await reservas.GerarRelatorioAsync();
    Console.WriteLine("Relatório:");
    Console.WriteLine($"  Reservas ativas: {relatorio.ReservasAtivas} | finalizadas: {relatorio.ReservasFinalizadas} | canceladas: {relatorio.ReservasCanceladas}");
    Console.WriteLine($"  Faturamento realizado: R$ {relatorio.FaturamentoRealizado:N2} | previsto: R$ {relatorio.FaturamentoPrevisto:N2}");
    Console.WriteLine($"  Tipo de quarto mais reservado: {relatorio.TipoMaisReservado}");
    Console.WriteLine();

    // 7º Cancelar a reserva da suíte (assim ela fica livre na próxima execução)
    reservaBruno = await reservas.CancelarAsync(reservaBruno.Id);
    Console.WriteLine($"Cancelada:  {reservaBruno}");
}
catch (HotelException ex)
{
    Console.WriteLine($"Operação recusada: {ex.Message}");
}

static async Task Tentar(Func<Task> acao)
{
    try
    {
        await acao();
        Console.WriteLine("  Aceita");
    }
    catch (HotelException ex)
    {
        Console.WriteLine($"  Recusada ({ex.GetType().Name}): {ex.Message}");
    }
}

static DateOnly ProximoDia(DayOfWeek dia)
{
    var data = DateOnly.FromDateTime(DateTime.Today);
    while (data.DayOfWeek != dia)
        data = data.AddDays(1);
    return data;
}
