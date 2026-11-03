namespace HotelCSharp.Model;

// Superclasse de todas as entidades. Como todas têm Id, um único repositório
// genérico (MemoriaRepository<T>) serve para hóspede, quarto e reserva.
public abstract class EntidadeBase
{
    // internal set: só os repositórios (mesmo projeto) definem o Id.
    public int Id { get; internal set; }
}
