namespace Backend.Data.Repositories
{
    using System;
    using Backend.Data.Entitites;
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
                .Include(t => t.Location)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<List<TaskItem>> GetAllFullAsync()
        {
            return await context.Tasks
                .Include(t => t.RequiredQualifications)
                    .ThenInclude(tq => tq.Qualification)
                .Include(t => t.RequiredTools)
                    .ThenInclude(tt => tt.Tool)
                .Include(t => t.Location)
                .ToListAsync();
        }

        public async Task<TaskSchedule?> GetTaskScheduleByTaskIdAsync(int taskId)
        {
            return await context.TaskSchedules
                .Where(ts => ts.TaskItemId == taskId)
                .Include(ts => ts.Boat)
                .ThenInclude(b => b.Plan)
                .OrderByDescending(ts => ts.Boat.Plan.CreatedAt)
                .FirstOrDefaultAsync();
        }
    }
}
