using Backend.Data.Entitites;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data.Repositories;

public static class PlanQueryExtensions
{
    public static IQueryable<Plan> IncludeFullPlanDetails(this IQueryable<Plan> query)
    {
        return query
            .Include(x => x.Boats)
                .ThenInclude(b => b.Persons)
                    .ThenInclude(bp => bp.Person)
            .Include(x => x.Boats)
                .ThenInclude(b => b.Tools)
            .Include(x => x.Boats)
                .ThenInclude(b => b.TaskSchedules)
                    .ThenInclude(ts => ts.TaskItem)
            .Include(x => x.Boats)
                .ThenInclude(b => b.BoatSchedules)
                    .ThenInclude(bs => bs.Origin)
            .Include(x => x.Boats)
                .ThenInclude(b => b.BoatSchedules)
                    .ThenInclude(bs => bs.Destination);
    }
}

public class PlanRepository : Repository<Plan>, IPlanRepository
{
    private readonly AppDbContext context;

    public PlanRepository(AppDbContext context) : base(context)
    {
        this.context = context;
    }

    public async Task<Plan?> GetFullPlanByIdAsync(int id)
    {
        var plan = await context.Plans
            .Where(p => p.Id == id)
            .IncludeFullPlanDetails()
            .AsSplitQuery()
            .FirstOrDefaultAsync();

        return plan;
    }

    public async Task<List<Plan>> GetAllFullAsync()
    {
        return await context.Plans
            .IncludeFullPlanDetails()
            .AsSplitQuery()
            .ToListAsync();
    }

    public async Task<Plan?> GetLatestPlanForDayAsync(DateOnly day)
    {
        var start = day;
        var end = start.AddDays(1);

        return await context.Plans
            .Where(p => p.Date >= start && p.Date < end)
            .OrderByDescending(p => p.Date)
            .IncludeFullPlanDetails()
            .AsSplitQuery()
            .FirstOrDefaultAsync();
    }

    public async Task<DateOnly?> GetLastDateWithPlansAsync()
    {
        return await context.Plans
            .OrderByDescending(p => p.Date)
            .Select(p => (DateOnly?)p.Date) // cast to nullable to avoid getting weird default data of 01.01.0001
            .FirstOrDefaultAsync();
    }
}
