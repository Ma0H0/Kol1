using Kolokwium1Poprawa.Models.DTOs;

namespace Kolokwium1Poprawa.Services;

public interface IDbService
{
    Task<AppointmentDTO> GetAppointmentById(int id);
    Task AddAppointment(AppointmentAddDTO appointment);
}