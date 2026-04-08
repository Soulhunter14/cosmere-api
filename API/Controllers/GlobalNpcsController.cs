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
    public async Task<ActionResult<List<GlobalNpcResponse>>> GetAll()
        => Ok(await service.GetAllAsync());

    [HttpGet("{id:long}")]
    public async Task<ActionResult<GlobalNpcResponse>> GetById(long id)
        => Ok(await service.GetByIdAsync(id));

    [HttpPost]
    public async Task<ActionResult<GlobalNpcResponse>> Create([FromBody] GlobalNpcRequest request)
        => Ok(await service.CreateAsync(request));

    [HttpPut("{id:long}")]
    public async Task<ActionResult<GlobalNpcResponse>> Update(long id, [FromBody] GlobalNpcRequest request)
        => Ok(await service.UpdateAsync(id, request));

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id)
    {
        await service.DeleteAsync(id);
        return NoContent();
    }

}
