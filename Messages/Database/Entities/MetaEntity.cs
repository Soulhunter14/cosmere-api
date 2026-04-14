namespace Messages.Database.Entities;

public class MetaEntity
{
    public long Id { get; set; }
    public long CharacterId { get; set; }
    public required string Titulo { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public int Hitos { get; set; } = 0;
    public string Estado { get; set; } = "activa"; // "activa" | "concluida"
    public string? TipoConclusion { get; set; }   // "exito" | "crecimiento" | "fracaso"
    public string NotasConclusion { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public CharacterEntity Character { get; set; } = null!;
}
