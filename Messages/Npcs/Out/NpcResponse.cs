namespace Messages.Npcs.Out;

public class NpcResponse
{
    public long Id { get; set; }
    public long CampaignId { get; set; }
    public required string Name { get; set; }
    public bool IsVisibleToPlayers { get; set; }
    public int Level { get; set; }
    public int Experience { get; set; }
    public string CaminoHeroico { get; set; } = string.Empty;
    public string CaminoRadiante { get; set; } = string.Empty;
    public string Ascendencia { get; set; } = string.Empty;
    public int Fuerza { get; set; }
    public int Velocidad { get; set; }
    public int Intelecto { get; set; }
    public int Voluntad { get; set; }
    public int Discernimiento { get; set; }
    public int Presencia { get; set; }
    public int Health { get; set; }
    public int MaxHealth { get; set; }
    public int Concentration { get; set; }
    public int MaxConcentration { get; set; }
    public int Investiture { get; set; }
    public int MaxInvestiture { get; set; }
    public int Desvio { get; set; }
    public int MarcosInfusas { get; set; }
    public int MarcosOpacas { get; set; }
    public int Agilidad { get; set; }
    public int ArmasLigeras { get; set; }
    public int ArmasPesadas { get; set; }
    public int Atletismo { get; set; }
    public int Hurto { get; set; }
    public int Sigilo { get; set; }
    public int Deduccion { get; set; }
    public int Disciplina { get; set; }
    public int Intimidacion { get; set; }
    public int Manufactura { get; set; }
    public int Medicina { get; set; }
    public int Conocimiento { get; set; }
    public int Engano { get; set; }
    public int Liderazgo { get; set; }
    public int Percepcion { get; set; }
    public int Perspicacia { get; set; }
    public int Persuasion { get; set; }
    public int Supervivencia { get; set; }
    public string HabilidadPersonalizada1 { get; set; } = string.Empty;
    public int HabilidadPersonalizada1Valor { get; set; }
    public string HabilidadPersonalizada1Atributo { get; set; } = string.Empty;
    public string HabilidadPersonalizada2 { get; set; } = string.Empty;
    public int HabilidadPersonalizada2Valor { get; set; }
    public string HabilidadPersonalizada2Atributo { get; set; } = string.Empty;
    public string HabilidadPersonalizada3 { get; set; } = string.Empty;
    public int HabilidadPersonalizada3Valor { get; set; }
    public string HabilidadPersonalizada3Atributo { get; set; } = string.Empty;
    public string Proposito { get; set; } = string.Empty;
    public string Obstaculo { get; set; } = string.Empty;
    public string Metas { get; set; } = string.Empty;
    public string Talentos { get; set; } = string.Empty;
    public string Apariencia { get; set; } = string.Empty;
    public string Notas { get; set; } = string.Empty;
    public string Conexiones { get; set; } = string.Empty;
    public List<string> Weapons { get; set; } = [];
    public List<string> Armor { get; set; } = [];
    public List<string> Spells { get; set; } = [];
    public List<string> Equipment { get; set; } = [];
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
