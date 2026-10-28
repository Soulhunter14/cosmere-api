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
/// «Creación de personajes»: el director libera los pasos uno a uno, permite o deniega cambiar los ya respondidos, crea las fichas asignadas
/// a los jugadores y las da por creadas; los jugadores leen el estado y rellenan su ficha con el <c>PUT</c> de personajes de siempre (el
/// servidor vigila los pasos liberados y los cambiables). Cada cambio avisa al grupo de la campaña con <c>CreacionUpdated</c> (sin datos:
/// cada cliente relee).
/// </summary>
[ApiController]
[Authorize]
[Route("campaigns/{campaignId:long}/creacion")]
public class CreacionController(ICreacionService creacion, IHubContext<CampaignHub> hubContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<CreacionResponse>> Get(long campaignId)
        => Ok(await creacion.GetAsync(campaignId, JwtHelper.GetUserId(User)));

    [HttpPut("paso")]
    public async Task<ActionResult<CreacionResponse>> SetPaso(long campaignId, [FromBody] CreacionPasoRequest request)
        => Ok(await Avisar(campaignId, await creacion.SetPasoAsync(campaignId, request.Paso, JwtHelper.GetUserId(User))));

    [HttpPut("cambiable")]
    public async Task<ActionResult<CreacionResponse>> SetCambiable(long campaignId, [FromBody] CreacionCambiableRequest request)
        => Ok(await Avisar(campaignId, await creacion.SetCambiableAsync(campaignId, request.Paso, request.Permitido, JwtHelper.GetUserId(User))));

    [HttpPost("personajes")]
    public async Task<ActionResult<CreacionResponse>> AddPersonaje(long campaignId, [FromBody] CreacionPersonajeRequest request)
        => Ok(await Avisar(campaignId, await creacion.AddPersonajeAsync(campaignId, request.OwnerId, JwtHelper.GetUserId(User))));

    [HttpPost("personajes/{characterId:long}/completar")]
    public async Task<ActionResult<CreacionResponse>> Completar(long campaignId, long characterId)
        => Ok(await Avisar(campaignId, await creacion.CompletarAsync(campaignId, characterId, true, JwtHelper.GetUserId(User))));

    [HttpDelete("personajes/{characterId:long}/completar")]
    public async Task<ActionResult<CreacionResponse>> Deshacer(long campaignId, long characterId)
        => Ok(await Avisar(campaignId, await creacion.CompletarAsync(campaignId, characterId, false, JwtHelper.GetUserId(User))));

    [HttpPost("sesion/preparar")]
    public async Task<ActionResult<CreacionResponse>> PrepararSesion(long campaignId)
        => Ok(await Avisar(campaignId, await creacion.SesionAsync(campaignId, SesionCreacion.Preparar, null, JwtHelper.GetUserId(User))));

    [HttpPost("sesion/iniciar")]
    public async Task<ActionResult<CreacionResponse>> IniciarSesion(long campaignId, [FromBody] SesionCreacionRequest? request)
        => Ok(await Avisar(campaignId, await creacion.SesionAsync(campaignId, SesionCreacion.Iniciar, request, JwtHelper.GetUserId(User))));

    [HttpPost("sesion/terminar")]
    public async Task<ActionResult<CreacionResponse>> TerminarSesion(long campaignId)
        => Ok(await Avisar(campaignId, await creacion.SesionAsync(campaignId, SesionCreacion.Terminar, null, JwtHelper.GetUserId(User))));

    private async Task<CreacionResponse> Avisar(long campaignId, CreacionResponse estado)
    {
        await hubContext.Clients.Group(CampaignHub.CampaignGroup(campaignId)).SendAsync("CreacionUpdated", new { campaignId });
        return estado;
    }
}
