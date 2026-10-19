namespace Messages.Characters;

/// <summary>
/// Lo que llevaba un personaje al morir: se le retira de la ficha y se guarda aquí (columna <c>EquipoAlMorir</c>) para devolvérselo si el
/// director lo devuelve a la vida. El núcleo guarda el equipo común; <see cref="Mundo"/> es lo que retiró el mundo de la campaña.
/// </summary>
public sealed class EquipoGuardado
{
    public List<string> Weapons { get; set; } = [];
    public List<string> Armor { get; set; } = [];
    public List<string> Spells { get; set; } = [];
    public List<string> Equipment { get; set; } = [];
    public string EquippedArmor { get; set; } = string.Empty;
    public int Desvio { get; set; }
    public int MarcosInfusas { get; set; }
    public int MarcosOpacas { get; set; }

    /// <summary>Lo que el mundo retiró, con claves que solo el mundo entiende (p. ej. "arquillas", "viales:alomancia:hierro").</summary>
    public Dictionary<string, decimal> Mundo { get; set; } = [];
}
