using System.Data.Common;
using Kolokwium1Poprawa.Exceptions;
using Kolokwium1Poprawa.Models.DTOs;
using Microsoft.Data.SqlClient;

namespace Kolokwium1Poprawa.Services;

public class DbService : IDbService
{
    private readonly string _connectionString;
    public DbService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Default") ?? string.Empty;
    }
    
    public async Task<AppointmentDTO> GetAppointmentById(int id)
    {
        var query =
            @"SELECT a.appoitment_id,a.date,p.first_name,p.last_name,p.date_of_birth,d.doctor_id,d.PWZ,s.name,sa.service_fee FROM Appointment a
            JOIN Patient p ON a.patient_id = p.patient_id
            JOIN Doctor d ON a.doctor_id = d.doctor_id
            JOIN Appointment_Service sa ON a.appoitment_id = sa.appoitment_id
            JOIN Service s ON sa.service_id = s.service_id
            WHERE a.appoitment_id = @id;";

        await using SqlConnection connection = new SqlConnection(_connectionString);
        await using SqlCommand command = new SqlCommand();
        
        command.Connection = connection;
        command.CommandText = query;
        await connection.OpenAsync();
        
        command.Parameters.AddWithValue("@id", id);
        var reader = await command.ExecuteReaderAsync();
        
        AppointmentDTO? appointment = null;
        while (await reader.ReadAsync())
        {
            if (appointment == null)
            {
                appointment = new AppointmentDTO
                {
                    Date = reader.GetDateTime(reader.GetOrdinal("date")),
                    Patient = new PatientDTO
                    {
                        FirstName = reader.GetString(reader.GetOrdinal("first_name")),
                        LastName = reader.GetString(reader.GetOrdinal("last_name")),
                        DateOfBirth = reader.GetDateTime(reader.GetOrdinal("date_of_birth")),
                    },
                    Doctor = new DoctorDTO
                    {
                        DoctorId = reader.GetInt32(reader.GetOrdinal("doctor_id")),
                        Pwz = reader.GetString(reader.GetOrdinal("PWZ")),
                    },
                    AppointmentServices = new List<ServiceDTO>()
                };
            }
            int appointmentId = reader.GetInt32(reader.GetOrdinal("appoitment_id"));
            
            var service = appointment.AppointmentServices.FirstOrDefault(e => e.Id.Equals(appointmentId));

                service = new ServiceDTO()
                {
                    Id = reader.GetInt32(reader.GetOrdinal("appoitment_id")),
                    Name = reader.GetString(reader.GetOrdinal("name")),
                    ServiceFee = reader.GetDecimal(reader.GetOrdinal("service_fee")),
                };
                appointment.AppointmentServices.Add(service);
            
        }

        if (appointment == null)
        {
            throw new NotFoundException("Appointment not found");
        }
        return appointment;

    }

    public async Task AddAppointment(AppointmentAddDTO appointment)
    {
        await using SqlConnection connection = new SqlConnection(_connectionString);
        await using SqlCommand command = new SqlCommand();
        
        command.Connection = connection;
        await connection.OpenAsync();
        
        DbTransaction transaction = await connection.BeginTransactionAsync();
        command.Transaction = transaction as SqlTransaction;

        try
        {
            command.Parameters.Clear();
            command.CommandText = "SELECT appoitment_id FROM Appointment WHERE appoitment_id = @id;";
            command.Parameters.AddWithValue("@id", appointment.AppointmentId);
            var appointmentIdRes = await command.ExecuteScalarAsync();
            if(appointmentIdRes != null)
                throw new ConflictException($"Appointmnet with ID - {appointment.AppointmentId} - already exists.");
            
            command.Parameters.Clear();
            command.CommandText = "SELECT 1 FROM Patient WHERE patient_id = @IdPatient;";
            command.Parameters.AddWithValue("@IdPatient", appointment.PatientId);
                
            var patientIdRes = await command.ExecuteScalarAsync();
            if(patientIdRes is null)
                throw new NotFoundException($"Patient with ID - {appointment.PatientId} - not found.");
            
            command.Parameters.Clear();
            command.CommandText = "SELECT doctor_id FROM Doctor WHERE PWZ = @PWZ;";
            command.Parameters.AddWithValue("@PWZ", appointment.PWZ);
                
            var PWZRes = await command.ExecuteScalarAsync();
            if(PWZRes is null)
                throw new NotFoundException($"Doctor with PWZ - {appointment.PWZ} - not found.");
            
            command.Parameters.Clear();
            command.CommandText = 
                @"INSERT INTO Appointment
                VALUES (@appointment_id, @patient_id, @PWZ, @date)";
            
            command.Parameters.AddWithValue("@appointment_id", appointment.AppointmentId);
            command.Parameters.AddWithValue("@patient_id", appointment.PatientId);
            command.Parameters.AddWithValue("@PWZ", PWZRes);
            command.Parameters.AddWithValue("@date", DateTime.Now); 
            
            await command.ExecuteNonQueryAsync();
            await transaction.CommitAsync();
            
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            throw;
        }
        
    }
}