namespace Messages.Database.Entities;

public class GlobalNpcEntity
{
    public long Id { get; set; }
    public required string Name { get; set; }
    public string Source { get; set; } = string.Empty;   // e.g. "Caminapiedras"
    public string Tipo { get; set; } = string.Empty;     // e.g. "Secuaz Rango 1"
    public string Ascendencia { get; set; } = string.Empty;

    public int Level { get; set; } = 1;

    // Core Attributes
    public int Fuerza { get; set; } = 0;
    public int Velocidad { get; set; } = 0;
    public int Intelecto { get; set; } = 0;
    public int Voluntad { get; set; } = 0;
    public int Discernimiento { get; set; } = 0;
    public int Presencia { get; set; } = 0;

    // Resources
    public int MaxHealth { get; set; } = 10;
    public int MaxConcentration { get; set; } = 0;
    public int MaxInvestiture { get; set; } = 0;

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

    // Lore
    public string Talentos { get; set; } = string.Empty;  // Rasgos / traits
    public string Apariencia { get; set; } = string.Empty;
    public string Notas { get; set; } = string.Empty;     // Actions, tactics, etc.

    public string? ImageUrl { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
