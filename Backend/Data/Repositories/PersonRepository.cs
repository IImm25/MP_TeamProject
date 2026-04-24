using Backend.Data;
using Backend.Data.DTO;
using Microsoft.EntityFrameworkCore;

namespace Backend.Web.Repositories;

public class PersonRepository : IPersonRepository
{
    private readonly AppDbContext _db;

    public PersonRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<Person>> GetAllAsync()
        => await _db.People
            .Include(p => p.Qualifications)
                .ThenInclude(pq => pq.Qualification)
            .AsNoTracking()
            .ToListAsync();

    public async Task<Person?> GetByIdAsync(int id)
        => await _db.People
            .Include(p => p.Qualifications)
                .ThenInclude(pq => pq.Qualification)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);

    public async Task<Person> CreateAsync(PersonCreateDto dto)
    {
        var person = new Person(dto.Firstname, dto.Lastname);

        foreach (var qualId in dto.QualificationIds)
            person.Qualifications.Add(new PersonQualification { QualificationId = qualId });

        _db.People.Add(person);
        await _db.SaveChangesAsync();

        // Navigation-Properties nachladen
        return (await GetByIdAsync(person.Id))!;
    }

    public async Task<Person?> UpdateAsync(int id, PersonUpdateDto dto)
    {
        var person = await _db.People.FindAsync(id);
        if (person is null) return null;

        if (!string.IsNullOrWhiteSpace(dto.Firstname)) person.Firstname = dto.Firstname;
        if (!string.IsNullOrWhiteSpace(dto.Lastname)) person.Lastname = dto.Lastname;

        await _db.SaveChangesAsync();

        // Navigation-Properties nachladen
        return await GetByIdAsync(id);
    }

    public async Task<Person?> DeleteAsync(int id)
    {
        var person = await GetByIdAsync(id);
        if (person is null) return null;

        _db.People.Remove(_db.People.Find(id)!);
        await _db.SaveChangesAsync();
        return person;
    }
}
