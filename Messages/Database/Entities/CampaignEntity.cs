using Messages.Worlds;

namespace Messages.Database.Entities;

public class CampaignEntity
{
    public long Id { get; set; }
    public required string Name { get; set; }
    public long GmUserId { get; set; }
    public required string InviteCode { get; set; }
    public bool InviteActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string World { get; set; } = WorldIds.Stormlight;
    public string? Era { get; set; }
    /// <summary>Cuándo el director inició la campaña; <c>null</c> = en preparación (sesión 0). Ver <c>CierreCampana</c>.</summary>
    public DateTime? IniciadaEn { get; set; }
    /// <summary>
    /// Creación guiada de personajes (sección «Creación de personajes»): pasos liberados por el director, 0 = sin abrir. Los pasos los define
    /// cada mundo (<c>IWorldRules.PasosCreacion</c>); ver <c>CreacionGuiada</c>.
    /// </summary>
    public int CreacionPaso { get; set; }
    /// <summary>
    /// Pasos de la creación guiada que el director permite cambiar una vez respondidos (ids de campo de <c>CreacionGuiada</c>, p. ej. «legado»).
    /// Fuera de esta lista un jugador no cambia un paso que ya respondió (vaciarlo sí puede). El director lo conmuta desde «Creación».
    /// </summary>
    public List<string> CreacionCambiables { get; set; } = [];
    /// <summary>
    /// Sesión de creación (la sesión 0 de la pantalla del director, una por campaña): <c>pendiente</c> → <c>preparando</c> → <c>en-curso</c> →
    /// <c>jugada</c> (<c>SesionCreacionEstados</c>; transiciones en <c>SesionCreacion</c>). Mientras está en curso, las decisiones que la pantalla
    /// guarda en los personajes son de la creación. Guion y sesión de mesa con que se jugó, para la trazabilidad.
    /// </summary>
    public string SesionCreacionEstado { get; set; } = Creacion.SesionCreacionEstados.Pendiente;
    public DateTime? SesionCreacionPreparadaEn { get; set; }
    public DateTime? SesionCreacionIniciadaEn { get; set; }
    public DateTime? SesionCreacionTerminadaEn { get; set; }
    public string SesionCreacionGuion { get; set; } = string.Empty;
    public string SesionCreacionMesaId { get; set; } = string.Empty;

    public UserEntity GmUser { get; set; } = null!;
    public ICollection<CampaignMemberEntity> Members { get; set; } = [];
    public ICollection<CharacterEntity> Characters { get; set; } = [];
    public ICollection<SessionEntity> Sessions { get; set; } = [];
    public ICollection<SessionProposalEntity> Proposals { get; set; } = [];
}
