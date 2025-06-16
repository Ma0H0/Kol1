using Kolokwium1Poprawa.Exceptions;
using Kolokwium1Poprawa.Models.DTOs;
using Kolokwium1Poprawa.Services;
using Microsoft.AspNetCore.Mvc;

namespace Kolokwium1Poprawa.Controllers;

[Route("api/appointments")]
[ApiController]
public class AppointmentController : ControllerBase
{
    private readonly IDbService _dbService;
    public AppointmentController(IDbService dbService)
    {
        _dbService = dbService;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetAppointment(int id)
    {
        try
        {
            var res = await _dbService.GetAppointmentById(id);
            return Ok(res);
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
        
    }

    [HttpPost]
    public async Task<IActionResult> AddAppointment(AppointmentAddDTO appointment)
    {
        if (!appointment.Services.Any())
        {
            return BadRequest("At least one item is required.");
        }

        try
        {
            await _dbService.AddAppointment(appointment);
        }
        catch (ConflictException e)
        {
            return Conflict(e.Message);
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
            
        return Created("",appointment);
    }    
        
    }
    
    
