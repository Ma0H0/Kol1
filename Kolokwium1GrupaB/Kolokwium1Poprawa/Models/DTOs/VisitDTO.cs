namespace Kolokwium1Poprawa.Models.DTOs;

public class VisitDTO
{
    public DateTime Date { get; set; }
    public ClientDTO Client { get; set; }
    public MechanicDTO Mechanic { get; set; }
    public List<ServiceDTO> VisitServices { get; set; }
}

public class ClientDTO
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime DateOfBirth { get; set; }
    
}

public class MechanicDTO
{
    public int MechanicId { get; set; }
    public string LicenceNumber { get; set; }
}

public class ServiceDTO
{
    public int VisitId { get; set; }
    public string Name { get; set; }
    public decimal ServiceFee { get; set; }
    
}