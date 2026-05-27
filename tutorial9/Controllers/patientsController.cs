using Microsoft.AspNetCore.Mvc;
using tutorial9.DTOs;
using tutorial9.Services;

namespace tutorial9.Controllers;
[ApiController]
[Route("api/[controller]")]
public class patientsController : ControllerBase
{
    private readonly IDbService _service;

    public patientsController(IDbService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetPatients([FromQuery] string? search)
    {
        var patients = await _service.GetPatientsAsync(search);
        return Ok(patients);
    }

    [HttpPost("{pesel}/bedassignments")]
    public async Task<IActionResult> AssignBed(string pesel, [FromBody] BedAssignRequestDto request)
    {
        var result = await _service.AssignBedAsync(pesel, request);
        if (!result.IsSuccess)
        {
            return NotFound(new { message = result.Message });
        }
        return Ok(new { message = result.Message });
    }
}