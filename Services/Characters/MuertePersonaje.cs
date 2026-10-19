using Messages.Characters;
using Messages.Database.Entities;
using Services.Worlds;

namespace Services.Characters;

/// <summary>
/// Muerte de un personaje jugador (núcleo, ambos mundos): el director lo marca como muerto, la ficha queda de solo lectura y todo su
/// equipo (y su dinero) sale de la ficha a una copia, <c>EquipoAlMorir</c>, que se le devuelve si vuelve a la vida. El núcleo retira el
/// equipo común; lo propio de un mundo lo retira y lo devuelve ese mundo (<see cref="IWorldRules.RetirarEquipo"/>). Los clavos
/// hemalúrgicos no son equipo: se quedan en la ficha.
/// </summary>
public static class MuertePersonaje
{
    public const int MaxCausa = 300;

    /// <summary>Cualquier escritura sobre un muerto responde 409 (<see cref="InvalidOperationException"/>).</summary>
    public static void EnsureVivo(CharacterEntity c)
    {
        if (c.Muerto) throw new InvalidOperationException("This character is dead: their sheet is read-only.");
    }

    /// <summary>Retira el equipo común y el del mundo, lo guarda en <c>EquipoAlMorir</c> y marca al personaje como muerto.</summary>
    public static void Morir(CharacterEntity c, IWorldRules world, string? causa)
    {
        if (c.Muerto) throw new InvalidOperationException("This character is already dead.");
        var texto = (causa ?? "").Trim();
        if (texto.Length > MaxCausa) throw new ArgumentException($"The cause of death has at most {MaxCausa} characters.");

        var guardado = new EquipoGuardado
        {
            Weapons = [.. c.Weapons], Armor = [.. c.Armor], Spells = [.. c.Spells], Equipment = [.. c.Equipment],
            EquippedArmor = c.EquippedArmor, Desvio = c.Desvio,
            MarcosInfusas = c.MarcosInfusas, MarcosOpacas = c.MarcosOpacas,
        };
        // New lists, so that EF sees the change of the array columns
        c.Weapons = []; c.Armor = []; c.Spells = []; c.Equipment = [];
        c.EquippedArmor = string.Empty; c.Desvio = 0;
        c.MarcosInfusas = 0; c.MarcosOpacas = 0;
        guardado.Mundo = world.RetirarEquipo(c);

        c.EquipoAlMorir = CharacterJson.SerializarEquipoGuardado(guardado);
        c.Muerto = true;
        c.MuertoEn = DateTime.UtcNow;
        c.CausaMuerte = texto;
        c.UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>Devuelve exactamente lo guardado al morir (núcleo y mundo) y borra los datos de la muerte.</summary>
    public static void Revivir(CharacterEntity c, IWorldRules world)
    {
        if (!c.Muerto) throw new InvalidOperationException("This character is not dead.");

        // The sheet was read-only while dead, so what it holds now is what the death left: the copy goes back as it was
        if (CharacterJson.ParseEquipoGuardado(c.EquipoAlMorir) is { } guardado)
        {
            c.Weapons = [.. guardado.Weapons]; c.Armor = [.. guardado.Armor];
            c.Spells = [.. guardado.Spells]; c.Equipment = [.. guardado.Equipment];
            c.EquippedArmor = guardado.Armor.Contains(guardado.EquippedArmor) ? guardado.EquippedArmor : string.Empty;
            c.Desvio = guardado.Desvio;
            c.MarcosInfusas = guardado.MarcosInfusas; c.MarcosOpacas = guardado.MarcosOpacas;
            world.DevolverEquipo(c, guardado.Mundo);
        }

        c.EquipoAlMorir = "{}";
        c.Muerto = false;
        c.MuertoEn = null;
        c.CausaMuerte = string.Empty;
        c.UpdatedAt = DateTime.UtcNow;
    }
}
