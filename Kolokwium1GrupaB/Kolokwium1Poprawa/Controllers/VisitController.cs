using Kolokwium1Poprawa.Exceptions;
using Kolokwium1Poprawa.Models.DTOs;
using Kolokwium1Poprawa.Services;
using Microsoft.AspNetCore.Mvc;

namespace Kolokwium1Poprawa.Controllers;

[Route("api/visits")]
public class VisitController : ControllerBase
{
    private readonly IDbService _dbService;
    public VisitController(IDbService dbService)
    {
        _dbService = dbService;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetVisit(int id)
    {
        try
        {
            var res = await _dbService.GetVisitById(id);
            return Ok(res);
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
    }

    [HttpPost("")]
    public async Task<IActionResult> AddVisit([FromBody] VisitAddDTO visit)
    {
        if (!visit.Services.Any())
        {
            return BadRequest("At least one item is required.");
        }

        try
        {
            await _dbService.AddVisit(visit);
        }
        catch (ConflictException e)
        {
            return Conflict(e.Message);
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
            
        return Created("",visit);
        
    }
    
    
}