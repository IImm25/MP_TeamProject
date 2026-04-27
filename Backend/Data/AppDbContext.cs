using Microsoft.EntityFrameworkCore;

namespace Backend.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Person> Persons => Set<Person>();
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

        #region test
        // NEU – Beziehungen explizit definieren
        modelBuilder.Entity<PersonQualification>()
            .HasOne(pq => pq.Person)
            .WithMany(p => p.Qualifications)
            .HasForeignKey(pq => pq.PersonId);

        modelBuilder.Entity<PersonQualification>()
            .HasOne(pq => pq.Qualification)
            .WithMany()
            .HasForeignKey(pq => pq.QualificationId);
        #endregion


        modelBuilder.Entity<TaskQualification>()
            .HasKey(x => new { x.TaskItemId, x.QualificationId });

        modelBuilder.Entity<TaskTool>()
            .HasKey(x => new { x.TaskItemId, x.ToolId });

        modelBuilder.Entity<Qualification>().HasData(
            new Qualification { Id = 1, Name = "Electrical Systems Basics", Description = "Fundamental knowledge of electrical systems in wind turbines." },
            new Qualification { Id = 2, Name = "High Voltage Safety", Description = "Safe handling and operation in high-voltage environments." },
            new Qualification { Id = 3, Name = "Hydraulic Systems Maintenance", Description = "Inspection and repair of hydraulic turbine systems." },
            new Qualification { Id = 4, Name = "Mechanical Assembly & Repair", Description = "Repair of mechanical components like gearboxes and shafts." },
            new Qualification { Id = 5, Name = "Blade Inspection & Repair", Description = "Detecting and repairing structural blade damage." },
            new Qualification { Id = 6, Name = "SCADA Systems Operation", Description = "Monitoring turbines via SCADA systems." },
            new Qualification { Id = 7, Name = "Working at Heights Safety", Description = "Certified safety procedures for high-altitude work." },
            new Qualification { Id = 8, Name = "Gearbox Diagnostics", Description = "Fault detection and maintenance of gearboxes." },
            new Qualification { Id = 9, Name = "Electrical Fault Diagnosis", Description = "Identifying and resolving electrical failures." },
            new Qualification { Id = 10, Name = "Wind Turbine Installation Basics", Description = "Fundamentals of turbine assembly and installation." },

            new Qualification { Id = 11, Name = "Condition Monitoring Systems", Description = "Monitoring turbine health using sensor data." },
            new Qualification { Id = 12, Name = "Rotor Dynamics Understanding", Description = "Understanding vibration and rotor behavior." },
            new Qualification { Id = 13, Name = "Emergency Response Procedures", Description = "Handling turbine emergencies and shutdowns." },
            new Qualification { Id = 14, Name = "Lubrication Systems Maintenance", Description = "Maintaining lubrication systems for moving parts." },
            new Qualification { Id = 15, Name = "Nacelle Systems Maintenance", Description = "Servicing internal nacelle components." },
            new Qualification { Id = 16, Name = "Electrical Cabling & Wiring", Description = "Installation and repair of electrical wiring systems." },
            new Qualification { Id = 17, Name = "Power Conversion Systems", Description = "Maintenance of converters and inverters." },
            new Qualification { Id = 18, Name = "Grid Connection Systems", Description = "Connecting turbines to power grid infrastructure." },
            new Qualification { Id = 19, Name = "Turbine Control Systems", Description = "Understanding control logic and turbine automation." },
            new Qualification { Id = 20, Name = "Preventive Maintenance Planning", Description = "Planning and scheduling turbine maintenance tasks." }
        );
    }
}
