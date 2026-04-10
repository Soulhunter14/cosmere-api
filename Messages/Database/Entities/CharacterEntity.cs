namespace Messages.Database.Entities;

public class CharacterEntity
{
    public long Id { get; set; }
    public long CampaignId { get; set; }
    public long? OwnerId { get; set; }
    public bool IsNpc { get; set; } = false;
    public bool IsVisibleToPlayers { get; set; } = true;

    // Identity
    public required string Name { get; set; }
    public string PlayerName { get; set; } = string.Empty;

    // Progression
    public int Level { get; set; } = 1;
    public int Experience { get; set; } = 0;
    public string CaminoHeroico { get; set; } = string.Empty;
    public string CaminoRadiante { get; set; } = string.Empty;
    public string Ascendencia { get; set; } = string.Empty;

    // Core Attributes (0-5)
    public int Fuerza { get; set; } = 0;
    public int Velocidad { get; set; } = 0;
    public int Intelecto { get; set; } = 0;
    public int Voluntad { get; set; } = 0;
    public int Discernimiento { get; set; } = 0;
    public int Presencia { get; set; } = 0;

    // Resources
    public int Health { get; set; } = 10;
    public int MaxHealth { get; set; } = 10;
    public int MaxConcentration { get; set; } = 0;
    public int MaxInvestiture { get; set; } = 0;
    public int Desvio { get; set; } = 0;
    public int MarcosInfusas { get; set; } = 0;
    public int MarcosOpacas { get; set; } = 0;

    // Skills
    public int Agilidad { get; set; } = 0;
    public int ArmasLigeras { get; set; } = 0;
    public int ArmasPesadas { get; set; } = 0;
    public int Atletismo { get; set; } = 0;
    public int Hurto { get; set; } = 0;
    public int Sigilo { get; set; } = 0;
    public int Deduccion { get; set; } = 0;
    public int Disciplina { get; set; } = 0;
    public int Intimidacion { get; set; } = 0;
    public int Manufactura { get; set; } = 0;
    public int Medicina { get; set; } = 0;
    public int Conocimiento { get; set; } = 0;
    public int Engano { get; set; } = 0;
    public int Liderazgo { get; set; } = 0;
    public int Percepcion { get; set; } = 0;
    public int Perspicacia { get; set; } = 0;
    public int Persuasion { get; set; } = 0;
    public int Supervivencia { get; set; } = 0;
    public string HabilidadPersonalizada1 { get; set; } = string.Empty;
    public int HabilidadPersonalizada1Valor { get; set; } = 0;
    public string HabilidadPersonalizada1Atributo { get; set; } = string.Empty;
    public string HabilidadPersonalizada2 { get; set; } = string.Empty;
    public int HabilidadPersonalizada2Valor { get; set; } = 0;
    public string HabilidadPersonalizada2Atributo { get; set; } = string.Empty;
    public string HabilidadPersonalizada3 { get; set; } = string.Empty;
    public int HabilidadPersonalizada3Valor { get; set; } = 0;
    public string HabilidadPersonalizada3Atributo { get; set; } = string.Empty;
    public string HabilidadPersonalizada4 { get; set; } = string.Empty;
    public int HabilidadPersonalizada4Valor { get; set; } = 0;
    public string HabilidadPersonalizada4Atributo { get; set; } = string.Empty;
    public string HabilidadPersonalizada5 { get; set; } = string.Empty;
    public int HabilidadPersonalizada5Valor { get; set; } = 0;
    public string HabilidadPersonalizada5Atributo { get; set; } = string.Empty;
    public string HabilidadPersonalizada6 { get; set; } = string.Empty;
    public int HabilidadPersonalizada6Valor { get; set; } = 0;
    public string HabilidadPersonalizada6Atributo { get; set; } = string.Empty;

    // Roleplay
    public string Proposito { get; set; } = string.Empty;
    public string Obstaculo { get; set; } = string.Empty;
    public string Metas { get; set; } = string.Empty;
    public string Talentos { get; set; } = string.Empty;
    public string Apariencia { get; set; } = string.Empty;
    public string Notas { get; set; } = string.Empty;
    public string Conexiones { get; set; } = string.Empty;

    // Equipment (stored as text arrays)
    public List<string> Weapons { get; set; } = [];
    public List<string> Armor { get; set; } = [];
    public List<string> Spells { get; set; } = [];
    public List<string> Equipment { get; set; } = [];
    public string EquippedArmor { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public CampaignEntity Campaign { get; set; } = null!;
}
