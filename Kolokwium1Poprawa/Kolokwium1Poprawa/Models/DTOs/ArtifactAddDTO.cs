namespace Kolokwium1Poprawa.Models.DTOs;

public class ArtifactAddDTO
{
    public ArtifactADTO Artifact { get; set; }
    public ProjectAddDTO Project { get; set; }
}

public class ProjectAddDTO
{
    public int ProjectId { get; set; }
    public string Objective { get; set; }
    public DateOnly startDate { get; set; }
    public DateOnly? endDate { get; set; }
    
}

public class ArtifactADTO
{
    public int ArtifactId { get; set; }
    public string Name { get; set; }
    public DateOnly OriginDate { get; set; }
    public int InstitutionId { get; set; }
}