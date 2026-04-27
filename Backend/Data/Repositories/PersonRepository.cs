using Microsoft.EntityFrameworkCore;
using System;

namespace Backend.Data.Repositories
{
    public class PersonRepository : Repository<Person>, IPersonRepository
    {
        private readonly AppDbContext context;

        public PersonRepository(AppDbContext context) : base(context)
        {
            this.context = context;
        }

        public async Task<Person?> GetFullByIdAsync(int id)
        {
            var person = await context.Persons
                .Include(p => p.Qualifications)
                .ThenInclude(pq => pq.Qualification)
                .FirstOrDefaultAsync(p => p.Id == id);

            // Debug
            Console.WriteLine($"Person: {person?.Firstname}");
            Console.WriteLine($"Qualifications count: {person?.Qualifications?.Count ?? 0}");
            return person;
        }

        public async Task<List<Person>> GetAllFullAsync()
        {
            return await context.Persons
                .Include(p => p.Qualifications)
                .ThenInclude(pq => pq.Qualification)
                .ToListAsync();
        }
    }
}
