using System.Data.Common;
using Kolokwium1Poprawa.Exceptions;
using Kolokwium1Poprawa.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace Kolokwium1Poprawa.Services;

public class DbService : IDbService
{
    private readonly string _connectionString;
    public DbService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Default") ?? string.Empty;
    }
    
    public async Task<VisitDTO> GetVisitById(int id)
    {
        var query = 
                @"SELECT v.visit_id,v.date,c.first_name,c.last_name,c.date_of_birth,m.mechanic_id,m.licence_number,s.name,sv.service_fee FROM Visit v
                JOIN Client c ON c.client_id = v.client_id
                JOIN Mechanic m ON m.mechanic_id = v.mechanic_id
                JOIN Visit_Service sv ON sv.visit_id = v.visit_id
                JOIN Service s ON s.service_id = sv.service_id
                WHERE v.visit_id = @id";
        
        await using SqlConnection connection = new SqlConnection(_connectionString);
        await using SqlCommand command = new SqlCommand();
        
        command.Connection = connection;
        command.CommandText = query;
        await connection.OpenAsync();
        
        command.Parameters.AddWithValue("@id", id);
        var reader = await command.ExecuteReaderAsync();
        
        VisitDTO? visit = null;
        while (await reader.ReadAsync())
        {
            if (visit == null)
            {
                visit = new VisitDTO
                {
                    Date = reader.GetDateTime(reader.GetOrdinal("date")),
                    Client = new ClientDTO()
                    {
                        FirstName = reader.GetString(reader.GetOrdinal("first_name")),
                        LastName = reader.GetString(reader.GetOrdinal("last_name")),
                        DateOfBirth = reader.GetDateTime(reader.GetOrdinal("date_of_birth")),
                    },
                    Mechanic = new MechanicDTO()
                    {
                        MechanicId = reader.GetInt32(reader.GetOrdinal("mechanic_id")),
                        LicenceNumber = reader.GetString(reader.GetOrdinal("licence_number")),
                    },
                    VisitServices = new List<ServiceDTO>()
                };
            }
            int visitId = reader.GetInt32(reader.GetOrdinal("visit_id"));
            
            var service = visit.VisitServices.FirstOrDefault(e => e.VisitId.Equals(visitId));

            service = new ServiceDTO()
            {
                VisitId = visitId,
                Name = reader.GetString(reader.GetOrdinal("name")),
                ServiceFee = reader.GetDecimal(reader.GetOrdinal("service_fee")),
            };
            visit.VisitServices.Add(service);

        }

        if (visit == null)
        {
            throw new NotFoundException("Visit not found");
        }
        return visit;
    }

    public async Task AddVisit(VisitAddDTO visit)
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
            command.CommandText = "SELECT visit_id FROM Visit WHERE visit_id = @id;";
            command.Parameters.AddWithValue("@id", visit.VisitId);
            var visitIdRes = await command.ExecuteScalarAsync();
            if(visitIdRes != null)
                throw new ConflictException($"Visit with ID - {visit.VisitId} - already exists.");
            
            command.Parameters.Clear();
            command.CommandText = "SELECT client_id FROM Client WHERE client_id = @id;";
            command.Parameters.AddWithValue("@id", visit.ClientId);
            var clientIdRes = await command.ExecuteScalarAsync();
            if(clientIdRes != null)
                throw new ConflictException($"Client with ID - {visit.ClientId} - does not exist.");
            
            command.Parameters.Clear();
            command.CommandText = "SELECT mechanic_id FROM Mechanic WHERE licence_number = @ln;";
            command.Parameters.AddWithValue("@ln", visit.MechanicLicenceNumber);
            var appointmentIdRes = await command.ExecuteScalarAsync();
            if(appointmentIdRes != null)
                throw new ConflictException($"Mechanic with ID - {appointmentIdRes} - does not exist.");
            
            command.Parameters.Clear();
            command.CommandText =
                @"INSERT INTO Visit
                 VALUES (@visit_id,@client_id,@mechanic_id,@date)";
            command.Parameters.AddWithValue("@visit_id", visit.VisitId);
            command.Parameters.AddWithValue("@client_id", visit.ClientId);
            command.Parameters.AddWithValue("@mechanic_id", appointmentIdRes);
            command.Parameters.AddWithValue("@date", DateTime.Now);
            
            await command.ExecuteNonQueryAsync();
            await transaction.CommitAsync();

        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        
    }
}