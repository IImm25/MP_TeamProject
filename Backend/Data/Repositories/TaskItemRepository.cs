namespace Backend.Data.Repositories
{
    using System;
    using Backend.Data.Entitites;
    using Microsoft.EntityFrameworkCore;

    public static class TaskItemQueryExtensions
    {
        public static IQueryable<TaskItem> IncludeFullTaskItemDetails(this IQueryable<TaskItem> query)
        {
            return query
                .Include(t => t.RequiredQualifications)
                    .ThenInclude(tq => tq.Qualification)
                .Include(t => t.RequiredTools)
                    .ThenInclude(tt => tt.Tool)
                .Include(t => t.Location);
        }
    }


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
                .Where(t => t.Id == id)
                .IncludeFullTaskItemDetails()
                .FirstOrDefaultAsync();
        }

        public async Task<List<TaskItem>> GetAllFullAsync()
        {
            return await context.Tasks
                .IncludeFullTaskItemDetails()
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

        public async Task<List<TaskItem>> GetAllByLocationAsync(int locationId)
        {
            return await context.Tasks
                .Where(t => t.LocationId == locationId)
                .IncludeFullTaskItemDetails()
                .ToListAsync();
        }

        public async Task<List<TaskItem>> GetAllUncompletedTasksByDateAsync(DateOnly date)
        {
            return await context.Tasks
                .Where(t => t.ExecutionIntervalStart <= date && !t.IsCompleted)
                .IncludeFullTaskItemDetails()
                .ToListAsync();
        }

        public async Task<List<TaskItem>> GetAllScheduledTasksByDateAsync(DateOnly date)
        {
            var newestPlanIdQuery = context.Plans
                .Where(p => p.Date == date)
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => p.Id)
                .Take(1);

            return await context.Tasks
                .Where(t => context.TaskSchedules
                    .Where(ts => newestPlanIdQuery.Contains(ts.PlanId))
                    .Select(ts => ts.TaskItemId)
                    .Contains(t.Id))
                .IncludeFullTaskItemDetails()
                .ToListAsync();
        }

        public async Task<List<TaskItem>> GetAllOpenUnscheduledTasksByDateAsync(DateOnly date)
        {
            var newestPlanIdQuery = context.Plans
                .Where(p => p.Date == date)
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => p.Id)
                .Take(1);

            return await context.Tasks
                .Where(t => t.ExecutionIntervalStart <= date && !t.IsCompleted)
                .Where(t => !context.TaskSchedules
                    .Where(ts => newestPlanIdQuery.Contains(ts.PlanId))
                    .Select(ts => ts.TaskItemId)
                    .Contains(t.Id))
                .IncludeFullTaskItemDetails()
                .ToListAsync();
        }

        public async Task<List<TaskSchedule>> GetAllOngoingTasksByDateAndTimeAsync(DateOnly date, TimeOnly referenceTime)
        {
            var newestPlanIdQuery = context.Plans
                .Where(p => p.Date == date)
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => p.Id)
                .Take(1);

            TimeSpan targetTimeSpan = referenceTime.ToTimeSpan();
            double targetMs = targetTimeSpan.TotalMilliseconds;

            return await context.TaskSchedules
                .Where(ts => newestPlanIdQuery.Contains(ts.PlanId))
                .Where(ts => ts.StartTime <= referenceTime)
                .Where(ts => (targetTimeSpan - ts.StartTime.ToTimeSpan()).TotalHours < (double)ts.TaskItem.DurationHours)

                .Include(ts => ts.TaskItem)
                    .ThenInclude(t => t.RequiredQualifications)
                        .ThenInclude(tq => tq.Qualification)
                .Include(ts => ts.TaskItem)
                    .ThenInclude(t => t.RequiredTools)
                        .ThenInclude(tt => tt.Tool)
                .Include(ts => ts.TaskItem)
                    .ThenInclude(t => t.Location)
                .Include(ts => ts.Boat)
                    .ThenInclude(b => b.Persons)
                .Include(ts => ts.Boat)
                    .ThenInclude(b => b.Tools)
                .ToListAsync();
        }

        public async Task<List<TaskItem>> GetAllScheduledNotOngoingTasksByDateAndTimeAsync(DateOnly date, TimeOnly referenceTime)
        {
            var newestPlanIdQuery = context.Plans
                .Where(p => p.Date == date)
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => p.Id)
                .Take(1);

            TimeSpan targetTimeSpan = referenceTime.ToTimeSpan();
            double targetMs = referenceTime.ToTimeSpan().TotalMilliseconds;

            return await context.Tasks
                .Where(t => context.TaskSchedules
                    .Where(ts => newestPlanIdQuery.Contains(ts.PlanId))
                    .Where(ts => ts.StartTime > referenceTime || (targetTimeSpan - ts.StartTime.ToTimeSpan()).TotalHours >= (double)ts.TaskItem.DurationHours)
                    .Select(ts => ts.TaskItemId)
                    .Contains(t.Id))
                .IncludeFullTaskItemDetails()
                .ToListAsync();
        }

        public async Task SetAllScheduledTasksBeforeDateTimeAsCompleted(DateTime dateTime)
        {
            DateOnly targetDate = DateOnly.FromDateTime(dateTime);
            TimeSpan targetTimeSpan = TimeOnly.FromDateTime(dateTime).ToTimeSpan();

            var newestPlanIdQuery = context.Plans
                .Where(p => p.Date == targetDate)
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => p.Id)
                .Take(1);


            var todayCompletedTaskIds = await context.TaskSchedules
                .Where(ts => newestPlanIdQuery.Contains(ts.PlanId))
                .Where(ts => (targetTimeSpan - ts.StartTime.ToTimeSpan()).TotalHours >= (double)ts.TaskItem.DurationHours)
                .Select(ts => ts.TaskItemId)
                .ToListAsync();

            var pastScheduledTaskIds = await context.TaskSchedules
                .Where(ts => ts.Boat.Plan.Date < targetDate)
                .Select(ts => ts.TaskItemId)
                .ToListAsync();

            var allCompletedTaskIds = todayCompletedTaskIds.Union(pastScheduledTaskIds).ToList();

            var tasksToUpdate = await context.Tasks
                .Where(t => !t.IsCompleted && allCompletedTaskIds.Contains(t.Id))
                .ToListAsync();

            if (tasksToUpdate.Count == 0) return;

            foreach (var task in tasksToUpdate)
            {
                task.IsCompleted = true;
            }

            await context.SaveChangesAsync();
        }
    }
}
