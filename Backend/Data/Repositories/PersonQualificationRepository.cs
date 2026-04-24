using Backend.Data;
using Microsoft.EntityFrameworkCore;

namespace Backend.Web.Repositories;

public class PersonQualificationRepository : IPersonQualificationRepository
{
    private readonly AppDbContext _db;

    public PersonQualificationRepository(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>Alle Qualifikationen einer Person.</summary>
    public async Task<List<Qualification>> GetQualificationsByPersonAsync(int personId)
        => await _db.PersonQualifications
            .Where(pq => pq.PersonId == personId)
            .Include(pq => pq.Qualification)
            .AsNoTracking()
            .Select(pq => pq.Qualification)
            .ToListAsync();

    /// <summary>
    /// Verknüpft eine Qualifikation mit einer Person.
    /// Gibt null zurück wenn Person oder Qualifikation nicht existieren,
    /// oder die Verknüpfung bereits vorhanden ist.
    /// </summary>
    public async Task<PersonQualification?> AddAsync(int personId, int qualificationId)
    {
        var personExists = await _db.People.AnyAsync(p => p.Id == personId);
        var qualExists = await _db.Qualifications.AnyAsync(q => q.Id == qualificationId);

        if (!personExists || !qualExists) return null;

        var alreadyLinked = await _db.PersonQualifications
            .AnyAsync(pq => pq.PersonId == personId && pq.QualificationId == qualificationId);

        if (alreadyLinked) return null;

        var link = new PersonQualification
        {
            PersonId = personId,
            QualificationId = qualificationId
        };

        _db.PersonQualifications.Add(link);
        await _db.SaveChangesAsync();
        return link;
    }

    /// <summary>Entfernt die Verknüpfung. Gibt null zurück wenn sie nicht existiert.</summary>
    public async Task<PersonQualification?> RemoveAsync(int personId, int qualificationId)
    {
        var link = await _db.PersonQualifications
            .FirstOrDefaultAsync(pq => pq.PersonId == personId && pq.QualificationId == qualificationId);

        if (link is null) return null;

        _db.PersonQualifications.Remove(link);
        await _db.SaveChangesAsync();
        return link;
    }
}
