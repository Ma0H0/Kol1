using System.Data;
using System.Data.Common;
using Azure.Core;
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
    
    public async Task<ProjectDTO> GetProjectById(int id)
    {
        var query =
            @"SELECT p.ProjectId,p.Objective,p.StartDate,p.EndDate,a.Name,a.OriginDate,i.InstitutionId,i.Name,i.FoundedYear,sa.StaffId,s.FirstName,s.LastName,s.HireDate,sa.Role FROM Preservation_Project p 
            JOIN Artifact a ON p.ArtifactId = a.ArtifactId
            JOIN Institution i ON a.InstitutionId = i.InstitutionId
            JOIN Staff_Assignment sa ON p.ProjectId = sa.ProjectId
            JOIN Staff s ON sa.StaffId = s.StaffId
            WHERE p.ProjectId = @ProjectId";
        
        await using SqlConnection connection = new SqlConnection(_connectionString);
        await using SqlCommand command = new SqlCommand();
        
        command.Connection = connection;
        command.CommandText = query;
        await connection.OpenAsync();
        
        command.Parameters.AddWithValue("@ProjectId", id);
        var reader = await command.ExecuteReaderAsync();

        ProjectDTO? project = null;
        while (await reader.ReadAsync())
        {
            if (project == null)
            {
                project = new ProjectDTO
                {
                    ProjectId = reader.GetInt32(reader.GetOrdinal("ProjectId")),
                    Objective = reader.GetString(reader.GetOrdinal("Objective")),
                    StartDate = DateOnly.FromDateTime(reader.GetDateTime(reader.GetOrdinal("StartDate"))),
                    EndDate = await reader.IsDBNullAsync(reader.GetOrdinal("EndDate")) ? null : DateOnly.FromDateTime(reader.GetDateTime(reader.GetOrdinal("EndDate"))),
                    Artifact = new ArtifactDTO
                    {
                        Name = reader.GetString(reader.GetOrdinal("Name")),
                        OriginDate = DateOnly.FromDateTime(reader.GetDateTime(reader.GetOrdinal("OriginDate"))),
                        Institution = new InstitutionDTO
                        {
                            InstitutionId = reader.GetInt32(reader.GetOrdinal("InstitutionId")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            FoundedYear = reader.GetInt32(reader.GetOrdinal("FoundedYear"))
                            
                        }
                    },
                    StaffAssignments = new List<AssignmentDTO>()
                };
            }

            int StaffId = reader.GetInt32(reader.GetOrdinal("StaffId"));
            var staff = project.StaffAssignments.FirstOrDefault(e => e.StaffId.Equals(StaffId));

            staff = new AssignmentDTO
            {
                StaffId = reader.GetInt32(reader.GetOrdinal("StaffId")),
                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                HireDate = DateOnly.FromDateTime(reader.GetDateTime(reader.GetOrdinal("HireDate"))),
                Role = reader.GetString(reader.GetOrdinal("Role")),
            };
            project.StaffAssignments.Add(staff);
        }

        if (project == null)
        {
            throw new NotFoundException("Project not found");
        }
        return project;
    }

    public async Task AddArtifact(ArtifactAddDTO artifact)
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
            command.CommandText = "INSERT INTO Artifact VALUES (@ArtifactId, @Name,@OriginDate,@InstitutionId)";
            command.Parameters.AddWithValue("@ArtifactId", artifact.Artifact.ArtifactId);
            command.Parameters.AddWithValue("@Name", artifact.Artifact.Name);
            command.Parameters.AddWithValue("@OriginDate", artifact.Artifact.OriginDate);
            command.Parameters.AddWithValue("@InstitutionId", artifact.Artifact.InstitutionId);
            
            await command.ExecuteNonQueryAsync();
            await transaction.CommitAsync();
            
            command.Parameters.Clear();
            command.CommandText = "INSERT INTO Preservation_Project VALUES (@ProjectId,@ArtifactId,@StartDate,@EndDate,@Objective)";
            command.Parameters.AddWithValue("@ProjectId", artifact.Project.ProjectId);
            command.Parameters.AddWithValue("@ArtifactId", artifact.Artifact.ArtifactId);
            command.Parameters.AddWithValue("@Objective",artifact.Project.Objective);
            command.Parameters.AddWithValue("@StartDate", artifact.Project.startDate);
            command.Parameters.AddWithValue("@EndDate", artifact.Project.endDate);
            
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