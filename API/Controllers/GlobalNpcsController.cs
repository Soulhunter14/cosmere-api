using Cross.Security;
using Messages.GlobalNpcs.In;
using Messages.GlobalNpcs.Out;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.GlobalNpcs;

namespace API.Controllers;

[ApiController]
[Route("global-npcs")]
[Authorize]
public class GlobalNpcsController(IGlobalNpcService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<GlobalNpcResponse>>> GetAll([FromQuery] long? campaignId)
        => Ok(await service.GetAllAsync(campaignId, JwtHelper.GetUserId(User)));

    [HttpGet("{id:long}")]
    public async Task<ActionResult<GlobalNpcResponse>> GetById(long id)
        => Ok(await service.GetByIdAsync(id));

    [HttpPost]
    public async Task<ActionResult<GlobalNpcResponse>> Create([FromBody] GlobalNpcRequest request, [FromQuery] long? campaignId)
        => Ok(await service.CreateAsync(request, campaignId, JwtHelper.GetUserId(User)));

    [HttpPut("{id:long}")]
    public async Task<ActionResult<GlobalNpcResponse>> Update(long id, [FromBody] GlobalNpcRequest request, [FromQuery] long? campaignId)
        => Ok(await service.UpdateAsync(id, request, campaignId, JwtHelper.GetUserId(User)));

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id, [FromQuery] long? campaignId)
    {
        await service.DeleteAsync(id, campaignId, JwtHelper.GetUserId(User));
        return NoContent();
    }

}
