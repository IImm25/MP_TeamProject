namespace Backend.Data.Repositories
{
    using Microsoft.EntityFrameworkCore;

    public class TaskItemRepository : Repository<TaskItem>, ITaskItemRepository
    {
        private readonly AppDbContext context;

        public TaskItemRepository(AppDbContext context) : base(context)
        {
            this.context = context;
        }

        public async Task<TaskItem?> GetFullByIdAsync(int id)
        {
            return await context.Tasks
                .Include(t => t.RequiredQualifications)
                    .ThenInclude(tq => tq.Qualification)
                .Include(t => t.RequiredTools)
                    .ThenInclude(tt => tt.Tool)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<List<TaskItem>> GetAllFullAsync()
        {
            return await context.Tasks
                .Include(t => t.RequiredQualifications)
                    .ThenInclude(tq => tq.Qualification)
                .Include(t => t.RequiredTools)
                    .ThenInclude(tt => tt.Tool)
                .ToListAsync();
        }
    }
}
