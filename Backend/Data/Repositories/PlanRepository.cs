using Backend.Data.Entitites;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data.Repositories;

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
        .AsSplitQuery()
            .FirstOrDefaultAsync(p => p.Id == id);

        return plan;
    }

    public async Task<List<Plan>> GetAllFullAsync()
    {         
        return await context.Plans
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
            .AsSplitQuery()
            .FirstOrDefaultAsync();
    }

}
