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

    public Task<List<PatientDto>> GetPatientsAsync(string? search)
    {
        throw new NotImplementedException();
    }

    public Task<BedAssignResult> AssignBedAsync(string pesel, BedAssignRequestDto request)
    {
        throw new NotImplementedException();
    }
}