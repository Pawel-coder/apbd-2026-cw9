namespace tutorial9.Services;
using tutorial9.DTOs;
public interface IDbService
{
    Task<List<PatientDto>> GetPatientsAsync(string? search);
    Task<BedAssignResult> AssignBedAsync(string pesel, BedAssignRequestDto request);
}