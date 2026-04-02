using Messages.Characters.In;
using Messages.Characters.Out;

namespace Services.Characters;

public interface ICharacterService
{
    Task<List<CharacterResponse>> GetCharactersAsync(long campaignId, long userId);
    Task<CharacterResponse> GetCharacterAsync(long characterId, long campaignId, long userId);
    Task<CharacterResponse> CreateCharacterAsync(long campaignId, CreateCharacterRequest request, long userId);
    Task<CharacterResponse> UpdateCharacterAsync(long characterId, long campaignId, UpdateCharacterRequest request, long userId);
    Task DeleteCharacterAsync(long characterId, long campaignId, long userId);
    Task<CharacterResponse> AssignCharacterAsync(long characterId, long campaignId, long? ownerId, long userId);
}
