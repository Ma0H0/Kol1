namespace Kolokwium1Poprawa.Models.DTOs;

public class ProjectDTO
{
    public int ProjectId { get; set; }
    public string Objective { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public ArtifactDTO Artifact { get; set; }
    public List <AssignmentDTO> StaffAssignments { get; set; }
}

public class ArtifactDTO
{
    public string Name { get; set; }
    public DateOnly OriginDate { get; set; }
    public InstitutionDTO Institution { get; set; }
}

public class AssignmentDTO
{
    public int StaffId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateOnly HireDate { get; set; }
    public string Role { get; set; }
}

public class InstitutionDTO
{
    public int InstitutionId { get; set; }
    public string Name { get; set; }
    public int FoundedYear { get; set; }
}