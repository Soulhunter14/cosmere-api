using Messages.Characters.In;
using Messages.Database.Entities;

namespace Services.Campaigns;

/// <summary>
/// Cierre de la campaña: mientras está en preparación (la sesión 0, <c>Campaign.IniciadaEn</c> nulo) los jugadores rellenan su personaje
/// libremente; cuando el director la inicia, los campos de esta lista quedan cerrados para ellos. El director puede editarlos siempre y
/// puede reabrir la preparación. Es una regla común a todos los mundos (núcleo), no de un mundo: un campo que un mundo no usa
/// (el legado en Stormlight) simplemente no cambia.
/// </summary>
public static class CierreCampana
{
    /// <summary>
    /// Campos del personaje cerrados al iniciar la campaña, con el nombre que tienen en el JSON (camelCase). Viajan en la respuesta de la
    /// campaña (<c>CamposDeCierre</c>) para que el cliente no repita la lista. Mantener al día con <see cref="ConservarCampos"/>.
    /// </summary>
    public static readonly IReadOnlyList<string> Campos = ["proposito", "obstaculo", "legado", "legadoRespuestas"];

    /// <summary>
    /// Bloqueo del <c>PUT</c> de un jugador con la campaña iniciada: conserva lo guardado en cada campo de <see cref="Campos"/>, igual que el
    /// núcleo conserva <c>Name</c> y <c>CaminoHeroico</c> (sin error, para que el resto de la ficha se guarde).
    /// </summary>
    public static void ConservarCampos(UpdateCharacterRequest request, CharacterEntity character)
    {
        request.Proposito = character.Proposito;
        request.Obstaculo = character.Obstaculo;
        request.Legado = character.Legado;
        request.LegadoRespuestas = character.LegadoRespuestas;
    }
}
