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
            .Include(x => x.PlanBoats)
            .ThenInclude(b => b.Persons)
                .ThenInclude(bp => bp.Person)
        .Include(x => x.PlanBoats)
            .ThenInclude(b => b.Tools)
        .Include(x => x.PlanBoats)
            .ThenInclude(b => b.TaskSchedules)
                .ThenInclude(ts => ts.TaskItem)
        .Include(x => x.PlanBoats)
            .ThenInclude(b => b.BoatSchedules)
        .AsSplitQuery()
            .FirstOrDefaultAsync(p => p.Id == id);

        return plan;
    }

    public async Task<List<Plan>> GetAllFullAsync()
    {         
        return await context.Plans
        .Include(x => x.PlanBoats)
            .ThenInclude(b => b.Persons)
                .ThenInclude(bp => bp.Person)
        .Include(x => x.PlanBoats)
            .ThenInclude(b => b.Tools)
        .Include(x => x.PlanBoats)
            .ThenInclude(b => b.TaskSchedules)
                .ThenInclude(ts => ts.TaskItem)
        .Include(x => x.PlanBoats)
            .ThenInclude(b => b.BoatSchedules)
        .AsSplitQuery()
        .ToListAsync();
    }

    
    public async Task DeleteFullAsync(int id)
    {
        var plan = await context.Plans.FindAsync(id);
        if (plan == null) return;
        context.Plans.Remove(plan);
        await context.SaveChangesAsync();
    }
}
