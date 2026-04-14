namespace Messages.Metas.In;

public class CreateMetaRequest
{
    public required string Titulo { get; set; }
    public string Descripcion { get; set; } = string.Empty;
}

public class UpdateMetaRequest
{
    public required string Titulo { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public int Hitos { get; set; }
}

public class ConcludeMetaRequest
{
    public required string TipoConclusion { get; set; } // "exito" | "crecimiento" | "fracaso"
    public string NotasConclusion { get; set; } = string.Empty;
}
