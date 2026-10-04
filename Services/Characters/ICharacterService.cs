using Messages.Characters.In;
using Messages.Characters.Out;

namespace Services.Characters;

public interface ICharacterService
{
    Task<List<CharacterResponse>> GetCharactersAsync(long campaignId, long userId);
    Task<CharacterResponse> GetCharacterAsync(long characterId, long campaignId, long userId, ContextoJuego ctx);
    Task<CharacterResponse> CreateCharacterAsync(long campaignId, CreateCharacterRequest request, long userId);
    Task<CharacterResponse> UpdateCharacterAsync(long characterId, long campaignId, UpdateCharacterRequest request, long userId);
    Task DeleteCharacterAsync(long characterId, long campaignId, long userId);
    Task<CharacterResponse> AssignCharacterAsync(long characterId, long campaignId, long? ownerId, long userId);

    // Estado de mesa (T13): PATCH recursos y acciones de dominio; las reglas son del mundo (IWorldRules.AplicarAccionMesa).
    Task<CharacterResponse> PatchRecursosAsync(long characterId, long campaignId, RecursosRequest request, long userId);
    Task<CharacterResponse> BeberVialAsync(long characterId, long campaignId, BeberVialRequest request, long userId);
    Task<CharacterResponse> InicioEscenaAsync(long characterId, long campaignId, InicioEscenaRequest request, long userId);
}
