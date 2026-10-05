namespace Messages.Characters.In;

public class CreateCharacterRequest
{
    public required string Name { get; set; }
    public string PlayerName { get; set; } = string.Empty;
    public int Level { get; set; } = 1;
    public string Ascendencia { get; set; } = string.Empty;
    public string CaminoHeroico { get; set; } = string.Empty;
    public string CaminoRadiante { get; set; } = string.Empty;
    public long? OwnerId { get; set; }
    public string CaminoMetal { get; set; } = string.Empty;
    public string CaminoInicial { get; set; } = string.Empty;
}

public class AssignCharacterRequest
{
    public long? OwnerId { get; set; }
}

public class UpdateCharacterRequest
{
    public required string Name { get; set; }
    public string PlayerName { get; set; } = string.Empty;
    public int Level { get; set; }
    public int Experience { get; set; }
    public string CaminoHeroico { get; set; } = string.Empty;
    public string CaminoRadiante { get; set; } = string.Empty;
    public string Ascendencia { get; set; } = string.Empty;
    public int IdealesJurados { get; set; }

    // Attributes
    public int Fuerza { get; set; }
    public int Velocidad { get; set; }
    public int Intelecto { get; set; }
    public int Voluntad { get; set; }
    public int Discernimiento { get; set; }
    public int Presencia { get; set; }

    // Resources
    public int MaxHealth { get; set; }
    public int MaxConcentration { get; set; }
    public int MaxInvestiture { get; set; }
    public int Desvio { get; set; }
    public int MarcosInfusas { get; set; }
    public int MarcosOpacas { get; set; }

    // Skills
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
    public string HabilidadPersonalizada4 { get; set; } = string.Empty;
    public int HabilidadPersonalizada4Valor { get; set; }
    public string HabilidadPersonalizada4Atributo { get; set; } = string.Empty;
    public string HabilidadPersonalizada5 { get; set; } = string.Empty;
    public int HabilidadPersonalizada5Valor { get; set; }
    public string HabilidadPersonalizada5Atributo { get; set; } = string.Empty;
    public string HabilidadPersonalizada6 { get; set; } = string.Empty;
    public int HabilidadPersonalizada6Valor { get; set; }
    public string HabilidadPersonalizada6Atributo { get; set; } = string.Empty;

    // Roleplay
    public string Proposito { get; set; } = string.Empty;
    public string Obstaculo { get; set; } = string.Empty;
    public string Talentos { get; set; } = string.Empty;
    public string Apariencia { get; set; } = string.Empty;
    public string Notas { get; set; } = string.Empty;
    public string Conexiones { get; set; } = string.Empty;

    // Equipment
    public List<string> Weapons { get; set; } = [];
    public List<string> Armor { get; set; } = [];
    public List<string> Spells { get; set; } = [];
    public List<string> Equipment { get; set; } = [];
    public string EquippedArmor { get; set; } = string.Empty;

    // Nacidos de la bruma: null = conservar el valor guardado (un cliente que no los envía no los pisa). Recursos y el
    // estado de mesa de los poderes no viajan aquí (PATCH …/recursos); de Poderes solo se escriben Arte, Metal, Origen y
    // MetaId, y el servidor normaliza el resto (CharacterJson.FusionarPoderes).
    public string? CaminoMetal { get; set; }
    public string? CaminoInicial { get; set; }
    public List<PoderPersonaje>? Poderes { get; set; }
    public List<string>? Bendiciones { get; set; }

    // Hemalurgia (T49a): null = conservar los clavos guardados. Los clavos son una recompensa del DJ (L.288 / PDF 294): en Nacidos
    // de la bruma solo los escribe el director (el servidor ignora los de un jugador no GM) y Stormlight no admite ninguno (400 si
    // la lista no está vacía). El poder de un clavo de poder viaja en Poderes con Origen = "clavo" (el servidor lo normaliza a
    // completo).
    public List<ClavoHemalurgico>? Clavos { get; set; }
}
