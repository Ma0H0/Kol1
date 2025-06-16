namespace Kolokwium1Poprawa.Models.DTOs;

public class VisitAddDTO
{
    public int VisitId { get; set; }
    public int ClientId { get; set; }
    public string MechanicLicenceNumber { get; set; }
    public List<ServiceAddDTO> Services { get; set; }
}

public class ServiceAddDTO
{
    public string ServiceName { get; set; }
    public decimal ServiceFee { get; set; }
}