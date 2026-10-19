namespace Messages.Characters.In;

/// <summary>Cuerpo de <c>POST …/characters/{id}/muerte</c>: la causa es opcional (máx. 300 caracteres).</summary>
public class MuerteRequest
{
    public string? Causa { get; set; }
}
