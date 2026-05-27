using Microsoft.EntityFrameworkCore;
using tutorial9.Models;

namespace tutorial9.Services;
using tutorial9.Data;
using tutorial9.DTOs;
public class DbService : IDbService
{
    private readonly HospitaldBContext _dbContext;

    public DbService(HospitaldBContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<PatientDto>> GetPatientsAsync(string? search)
    {
        var query = _dbContext.Patients
            .Include(p => p.Admissions)
            .ThenInclude(a => a.Ward)
            .Include(p => p.BedAssignments)
            .ThenInclude(ba => ba.Bed)
            .ThenInclude(b => b.BedType)
            .Include(p => p.BedAssignments)
            .ThenInclude(ba => ba.Bed)
            .ThenInclude(b => b.Room)
            .ThenInclude(r => r.Ward)
            .AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchTerm = $"%{search}%";
            query = query.Where(p => 
                EF.Functions.Like(p.FirstName, searchTerm) || 
                EF.Functions.Like(p.LastName, searchTerm));
        }
        var patients = await query.ToListAsync();
        return patients.Select(p => new PatientDto
        {
            Pesel = p.Pesel,
            FirstName = p.FirstName,
            LastName = p.LastName,
            Age = p.Age,
            Sex = p.Sex ? "Male" : "Female",
            Admissions = p.Admissions.Select(a => new AdmissionDto
            {
                Id = a.Id,
                AdmissionDate = a.AdmissionDate,
                DischargeDate = a.DischargeDate,
                Ward = new WardDto
                {
                    Id = a.Ward.Id,
                    Name = a.Ward.Name,
                    Description = a.Ward.Description
                }
            }).ToList(),
            BedAssignments = p.BedAssignments.Select(ba => new BedAssignmentDto
            {
                Id = ba.Id,
                From = ba.From,
                To = ba.To,
                Bed = new BedDto
                {
                    Id = ba.Bed.Id,
                    BedType = new BedTypeDto
                    {
                        Id = ba.Bed.BedType.Id,
                        Name = ba.Bed.BedType.Name,
                        Description = ba.Bed.BedType.Description
                    },
                    Room = new RoomDto
                    {
                        Id = ba.Bed.Room.Id,
                        HasTv = ba.Bed.Room.HasTv,
                        Ward = new WardDto
                        {
                            Id = ba.Bed.Room.Ward.Id,
                            Name = ba.Bed.Room.Ward.Name,
                            Description = ba.Bed.Room.Ward.Description
                        }
                    }
                }
            }).ToList()
        }).ToList();
    }

    public async Task<BedAssignResult> AssignBedAsync(string pesel, BedAssignRequestDto request)
    {
        var isPatientExisting = await _dbContext.Patients.AnyAsync(p=>p.Pesel == pesel);
        if (!isPatientExisting)
        {
            return new BedAssignResult
            {
                IsSuccess = false,
                Message = $"Pesel {pesel} does not exist."
            };
        }
        var bed = await _dbContext.Beds
            .Include(b => b.BedType)
            .Include(b => b.Room).ThenInclude(r => r.Ward)
            .Where(b => b.BedType.Name == request.BedType && b.Room.Ward.Name == request.Ward)
            .Where(b => !b.BedAssignments.Any(ba => 
                (request.To == null || ba.From < request.To) && 
                (ba.To == null || ba.To > request.From)         
            ))
            .FirstOrDefaultAsync();
        if (bed == null)
        {
            return new BedAssignResult
            {
                IsSuccess = false,
                Message = $"Bed of type {request.BedType} in ward {request.Ward} does not exist at the requested time."
            };
        }

        var assigment = new BedAssignment
        {
            PatientPesel = pesel,
            BedId = bed.Id,
            From = request.From,
            To = request.To
        };
        _dbContext.BedAssignments.Add(assigment);
        await _dbContext.SaveChangesAsync();
        return new BedAssignResult
        {
            IsSuccess = true,
            Message = $"Patient with pesel {pesel} is successfully assigned to Bed {bed.Id} in Room {bed.RoomId}."
        };
    }
}