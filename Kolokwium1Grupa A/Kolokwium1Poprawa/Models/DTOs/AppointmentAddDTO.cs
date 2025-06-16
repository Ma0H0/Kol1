namespace Kolokwium1Poprawa.Models.DTOs;

public class AppointmentAddDTO
{
    public int AppointmentId{ get; set; }
    public int PatientId { get; set; }
    public string PWZ { get; set; }
    public List<ServiceAddDTO> Services { get; set; }
    
}

public class ServiceAddDTO
{
    public string ServiceName { get; set; }
    public decimal ServiceFee { get; set; }
}