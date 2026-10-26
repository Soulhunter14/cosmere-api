using Messages.Creacion.Out;

namespace Services.Creacion;

/// <summary>Creación guiada de personajes (ver <c>CreacionGuiada</c>): el director libera pasos, crea fichas y las da por creadas.</summary>
public interface ICreacionService
{
    /// <summary>Estado para un miembro: el director ve todos los personajes; un jugador, solo los suyos.</summary>
    Task<CreacionResponse> GetAsync(long campaignId, long userId);

    /// <summary>Libera o retrocede pasos (director). Retroceder no borra lo escrito.</summary>
    Task<CreacionResponse> SetPasoAsync(long campaignId, int paso, long userId);

    /// <summary>Ficha nueva asignada a un jugador, con nombre provisional y en creación (director).</summary>
    Task<CreacionResponse> AddPersonajeAsync(long campaignId, long ownerId, long userId);

    /// <summary>«Personaje creado» (o deshacerlo) (director).</summary>
    Task<CreacionResponse> CompletarAsync(long campaignId, long characterId, bool completada, long userId);
}
