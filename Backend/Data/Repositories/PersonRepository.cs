using Microsoft.EntityFrameworkCore;

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
            return await context.Persons
                .Include(p => p.Qualifications)
                .ThenInclude(pq => pq.Qualification)
                .FirstOrDefaultAsync(p => p.Id == id);
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
