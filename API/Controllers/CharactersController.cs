using Cross.Security;
using Messages.Characters.In;
using Messages.Characters.Out;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Characters;

namespace API.Controllers;

[ApiController]
[Route("campaigns/{campaignId:long}/[controller]")]
[Authorize]
public class CharactersController(ICharacterService characterService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<CharacterResponse>>> GetCharacters(long campaignId)
        => Ok(await characterService.GetCharactersAsync(campaignId, JwtHelper.GetUserId(User)));

    [HttpGet("{characterId:long}")]
    public async Task<ActionResult<CharacterResponse>> GetCharacter(long campaignId, long characterId)
        => Ok(await characterService.GetCharacterAsync(characterId, campaignId, JwtHelper.GetUserId(User)));

    [HttpPost]
    public async Task<ActionResult<CharacterResponse>> CreateCharacter(long campaignId, [FromBody] CreateCharacterRequest request)
        => Ok(await characterService.CreateCharacterAsync(campaignId, request, JwtHelper.GetUserId(User)));

    [HttpPut("{characterId:long}")]
    public async Task<ActionResult<CharacterResponse>> UpdateCharacter(long campaignId, long characterId, [FromBody] UpdateCharacterRequest request)
        => Ok(await characterService.UpdateCharacterAsync(characterId, campaignId, request, JwtHelper.GetUserId(User)));

    [HttpDelete("{characterId:long}")]
    public async Task<IActionResult> DeleteCharacter(long campaignId, long characterId)
    {
        await characterService.DeleteCharacterAsync(characterId, campaignId, JwtHelper.GetUserId(User));
        return NoContent();
    }

    [HttpPut("{characterId:long}/assign")]
    public async Task<ActionResult<CharacterResponse>> AssignCharacter(long campaignId, long characterId, [FromBody] AssignCharacterRequest request)
        => Ok(await characterService.AssignCharacterAsync(characterId, campaignId, request.OwnerId, JwtHelper.GetUserId(User)));
}
