namespace Messages.Metas.Out;

public class MetaResponse
{
    public long Id { get; set; }
    public long CharacterId { get; set; }
    public required string Titulo { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public int Hitos { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string? TipoConclusion { get; set; }
    public string NotasConclusion { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
