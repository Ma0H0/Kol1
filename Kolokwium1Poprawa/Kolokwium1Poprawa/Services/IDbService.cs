using Kolokwium1Poprawa.Models.DTOs;

namespace Kolokwium1Poprawa.Services;

public interface IDbService
{
    Task<ProjectDTO> GetProjectById(int id); 
    Task AddArtifact(ArtifactAddDTO artifact);
}