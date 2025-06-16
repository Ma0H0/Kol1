using Kolokwium1Poprawa.Models.DTOs;

namespace Kolokwium1Poprawa.Services;

public interface IDbService
{
    Task<VisitDTO> GetVisitById(int id);
    
    Task AddVisit(VisitAddDTO visit);
    
}