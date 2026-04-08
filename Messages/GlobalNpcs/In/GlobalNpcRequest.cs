namespace Messages.GlobalNpcs.In;

public class GlobalNpcRequest
{
    public required string Name { get; set; }
    public string Source { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public string Ascendencia { get; set; } = string.Empty;
    public int Level { get; set; } = 1;
    public int Fuerza { get; set; }
    public int Velocidad { get; set; }
    public int Intelecto { get; set; }
    public int Voluntad { get; set; }
    public int Discernimiento { get; set; }
    public int Presencia { get; set; }
    public int MaxHealth { get; set; } = 10;
    public int MaxConcentration { get; set; }
    public int MaxInvestiture { get; set; }
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
    public string Talentos { get; set; } = string.Empty;
    public string Apariencia { get; set; } = string.Empty;
    public string Notas { get; set; } = string.Empty;
}
