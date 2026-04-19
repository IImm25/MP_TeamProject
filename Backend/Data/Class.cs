using Microsoft.EntityFrameworkCore;

namespace Backend.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Person> People => Set<Person>();
    public DbSet<TaskItem> Tasks => Set<TaskItem>();
    public DbSet<Tool> Tools => Set<Tool>();
    public DbSet<Qualification> Qualifications => Set<Qualification>();

    public DbSet<PersonQualification> PersonQualifications
        => Set<PersonQualification>();

    public DbSet<TaskQualification> TaskQualifications
        => Set<TaskQualification>();

    public DbSet<TaskTool> TaskTools
        => Set<TaskTool>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<PersonQualification>()
            .HasKey(x => new { x.PersonId, x.QualificationId });

        modelBuilder.Entity<TaskQualification>()
            .HasKey(x => new { x.TaskItemId, x.QualificationId });

        modelBuilder.Entity<TaskTool>()
            .HasKey(x => new { x.TaskItemId, x.ToolId });
    }
}
