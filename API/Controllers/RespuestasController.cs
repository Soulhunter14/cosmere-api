using API.Hubs;
using Cross.Security;
using Messages.Creacion.In;
using Messages.Creacion.Out;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Services.Creacion;

namespace API.Controllers;

/// <summary>
/// El registro de respuestas de un personaje (preguntas de la creación guiada y decisiones del guion de la pantalla del director; ver
/// <c>RespuestasPersonaje</c>). El director lee todas y guarda las decisiones del guion; un jugador solo lee las de su creación. Cada
/// cambio avisa con <c>CreacionUpdated</c>, como el resto de la creación.
/// </summary>
[ApiController]
[Authorize]
[Route("campaigns/{campaignId:long}/characters/{characterId:long}/respuestas")]
public class RespuestasController(IRespuestasService respuestas, IHubContext<CampaignHub> hubContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<RespuestaResponse>>> Get(long campaignId, long characterId)
        => Ok(await respuestas.GetAsync(campaignId, characterId, JwtHelper.GetUserId(User)));

    [HttpPut("decisiones")]
    public async Task<ActionResult<List<RespuestaResponse>>> PutDecisiones(long campaignId, long characterId, [FromBody] DecisionesPersonajeRequest request)
    {
        var lista = await respuestas.PutDecisionesAsync(campaignId, characterId, request, JwtHelper.GetUserId(User));
        await hubContext.Clients.Group(CampaignHub.CampaignGroup(campaignId)).SendAsync("CreacionUpdated", new { campaignId });
        return Ok(lista);
    }
}
