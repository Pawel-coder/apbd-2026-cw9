using Microsoft.AspNetCore.Mvc;
using tutorial9.Services;

namespace tutorial9.Controllers;
[ApiController]
[Route("api/[controller]")]
public class HospitalController : ControllerBase
{
    private readonly IDbService _service;

    public HospitalController(IDbService service)
    {
        _service = service;
    }
}