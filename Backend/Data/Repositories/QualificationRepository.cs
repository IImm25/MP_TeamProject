using Backend.Data.DBContext;
using Backend.Data.DTO;
using Microsoft.EntityFrameworkCore;

namespace Backend.Web.Repositories;
public class QualificationRepository : IQualificationRepository
{
    private readonly AppDbContext _db;

    public QualificationRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<Qualification>> GetAllAsync()
        => await _db.Qualifications
            .AsNoTracking()
            .ToListAsync();

    public async Task<Qualification?> GetByIdAsync(int id)
        => await _db.Qualifications
            .AsNoTracking()
            .FirstOrDefaultAsync(q => q.Id == id);

    public async Task<Qualification> CreateAsync(QualificationCreateDto dto)
    {
        var qualification = new Qualification(dto.Name, dto.Description);
        _db.Qualifications.Add(qualification);
        await _db.SaveChangesAsync();
        return qualification;
    }

    public async Task<Qualification?> UpdateAsync(int id, QualificationUpdateDto dto)
    {
        var qualification = await _db.Qualifications.FindAsync(id);
        if (qualification is null) return null;

        if (!string.IsNullOrWhiteSpace(dto.Name)) qualification.Name = dto.Name;
        if (!string.IsNullOrWhiteSpace(dto.Description)) qualification.Description = dto.Description;

        await _db.SaveChangesAsync();
        return qualification;
    }

    public async Task<Qualification?> DeleteAsync(int id)
    {
        var qualification = await _db.Qualifications.FindAsync(id);
        if (qualification is null) return null;

        _db.Qualifications.Remove(qualification);
        await _db.SaveChangesAsync();
        return qualification;
    }
}
