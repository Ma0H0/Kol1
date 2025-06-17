using Kolokwium1Poprawa.Exceptions;
using Kolokwium1Poprawa.Models.DTOs;
using Kolokwium1Poprawa.Services;
using Microsoft.AspNetCore.Mvc;

namespace Kolokwium1Poprawa.Controllers;

[Route("api/projects")]
[ApiController]
public class ProjectController: ControllerBase
{
    private readonly IDbService _dbService;
    public ProjectController(IDbService dbService)
    {
        _dbService = dbService;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetProject(int id)
    {
        try
        {
            var res = await _dbService.GetProjectById(id);
            return Ok(res);
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
        
    }

    [HttpPost("")]
    public async Task<IActionResult> AddArtifact(ArtifactAddDTO artifact)
    {
        try
        {
            await _dbService.AddArtifact(artifact);
        }
        catch (ConflictException e)
        {
            return Conflict(e.Message);
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
            
        return Created("",artifact);
    }
}