using Cross.Security;
using Messages.Metas.In;
using Messages.Metas.Out;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Metas;

namespace API.Controllers;

[ApiController]
[Route("campaigns/{campaignId:long}/characters/{characterId:long}/metas")]
[Authorize]
public class MetasController(IMetaService metaService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<MetaResponse>>> GetMetas(long campaignId, long characterId)
        => Ok(await metaService.GetMetasAsync(characterId, campaignId, JwtHelper.GetUserId(User)));

    [HttpPost]
    public async Task<ActionResult<MetaResponse>> CreateMeta(long campaignId, long characterId, [FromBody] CreateMetaRequest request)
        => Ok(await metaService.CreateMetaAsync(characterId, campaignId, request, JwtHelper.GetUserId(User)));

    [HttpPut("{metaId:long}")]
    public async Task<ActionResult<MetaResponse>> UpdateMeta(long campaignId, long characterId, long metaId, [FromBody] UpdateMetaRequest request)
        => Ok(await metaService.UpdateMetaAsync(metaId, characterId, campaignId, request, JwtHelper.GetUserId(User)));

    [HttpPost("{metaId:long}/conclude")]
    public async Task<ActionResult<MetaResponse>> ConcludeMeta(long campaignId, long characterId, long metaId, [FromBody] ConcludeMetaRequest request)
        => Ok(await metaService.ConcludeMetaAsync(metaId, characterId, campaignId, request, JwtHelper.GetUserId(User)));

    [HttpDelete("{metaId:long}")]
    public async Task<IActionResult> DeleteMeta(long campaignId, long characterId, long metaId)
    {
        await metaService.DeleteMetaAsync(metaId, characterId, campaignId, JwtHelper.GetUserId(User));
        return NoContent();
    }
}
