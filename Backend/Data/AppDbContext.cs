using Microsoft.EntityFrameworkCore;
using Backend.Data.Entitites;

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

	public DbSet<PersonQualification> PersonQualifications => Set<PersonQualification>();

	public DbSet<TaskQualification> TaskQualifications => Set<TaskQualification>();

	public DbSet<TaskTool> TaskTools => Set<TaskTool>();
	public DbSet<Turbine> Turbines => Set<Turbine>();

	public DbSet<Plan> Plans => Set<Plan>();
	public DbSet<PlanBoat> Boats => Set<PlanBoat>();

	public DbSet<BoatPerson> BoatPersons => Set<BoatPerson>();
	public DbSet<BoatTool> BoolTools => Set<BoatTool>();
	public DbSet<BoatSchedule> BoatSchedules => Set<BoatSchedule>();
	public DbSet<TaskSchedule> TaskSchedules => Set<TaskSchedule>();
	public DbSet<PlanQuery> PlanQueries => Set<PlanQuery>();


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.LogTo(Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Information);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);

		modelBuilder.Entity<TaskItem>()
			.HasOne(l => l.Location)
			.WithMany()
			.HasForeignKey(l => l.LocationId);


		modelBuilder.Entity<PersonQualification>()
			.HasKey(x => new { x.PersonId, x.QualificationId });

		modelBuilder.Entity<TaskQualification>()
			.HasKey(x => new { x.TaskItemId, x.QualificationId });

		modelBuilder.Entity<TaskTool>()
			.HasKey(x => new { x.TaskItemId, x.ToolId });

		// PlanBoat

		modelBuilder.Entity<PlanBoat>()
			.HasKey(x => new { x.PlanId, x.BoatNumber });

        modelBuilder.Entity<PlanBoat>()
			.HasOne(x => x.Plan)
			.WithMany(x => x.PlanBoats)
			.HasForeignKey(x => x.PlanId);

        // BoatPerson
        modelBuilder.Entity<BoatPerson>()
			.HasKey(x => new { x.PlanId, x.BoatNumber, x.PersonId });

		modelBuilder.Entity<BoatPerson>()
			.HasOne(x => x.Boat)
			.WithMany(x => x.Persons)
			.HasForeignKey(x => new { x.PlanId, x.BoatNumber });

		// BoatTool
		modelBuilder.Entity<BoatTool>()
			.HasKey(x => new { x.PlanId, x.BoatNumber, x.ToolId});

		modelBuilder.Entity<BoatTool>()
            .HasOne(x => x.Boat)
			.WithMany(x => x.Tools)
			.HasForeignKey(x => new { x.PlanId, x.BoatNumber });

		// BoatSchedule
        modelBuilder.Entity<BoatSchedule>()
			.HasKey(x => new { x.PlanId, x.BoatNumber, x.TripNumber });

        modelBuilder.Entity<BoatSchedule>()
			.HasOne(x => x.Boat)
			.WithMany(x => x.BoatSchedules)
			.HasForeignKey(x => new { x.PlanId, x.BoatNumber });

		// TaskSchedule
        modelBuilder.Entity<TaskSchedule>()
			.HasKey(x => new { x.PlanId, x.BoatNumber, x.TaskId });

		modelBuilder.Entity<TaskSchedule>()
			.HasOne(x => x.Boat)
			.WithMany(x => x.TaskSchedules)
			.HasForeignKey(x => new { x.PlanId, x.BoatNumber });



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
        modelBuilder.Entity<Turbine>().HasData(
            new Turbine { Id = 1,  Name = "B1",  Latitude = 54.61092f, Longitude = 12.63000f },
            new Turbine { Id = 2,  Name = "B2",  Latitude = 54.60553f, Longitude = 12.63000f },
            new Turbine { Id = 3,  Name = "B3",  Latitude = 54.60014f, Longitude = 12.63000f },
            new Turbine { Id = 4,  Name = "B4",  Latitude = 54.59475f, Longitude = 12.63000f },
            new Turbine { Id = 5,  Name = "B5",  Latitude = 54.58936f, Longitude = 12.63000f },
            new Turbine { Id = 6,  Name = "B6",  Latitude = 54.58397f, Longitude = 12.63000f },
            new Turbine { Id = 7,  Name = "B7",  Latitude = 54.61658f, Longitude = 12.64239f },
            new Turbine { Id = 8,  Name = "B8",  Latitude = 54.61119f, Longitude = 12.64239f },
            new Turbine { Id = 9,  Name = "B9",  Latitude = 54.60581f, Longitude = 12.64239f },
            new Turbine { Id = 10, Name = "B10", Latitude = 54.60042f, Longitude = 12.64239f },
            new Turbine { Id = 11, Name = "B11", Latitude = 54.59503f, Longitude = 12.64239f },
            new Turbine { Id = 12, Name = "B12", Latitude = 54.62033f, Longitude = 12.65475f },
            new Turbine { Id = 13, Name = "B13", Latitude = 54.61494f, Longitude = 12.65475f },
            new Turbine { Id = 14, Name = "B14", Latitude = 54.60956f, Longitude = 12.65475f },
            new Turbine { Id = 15, Name = "B15", Latitude = 54.60447f, Longitude = 12.65508f },
            new Turbine { Id = 16, Name = "B16", Latitude = 54.62358f, Longitude = 12.66714f },
            new Turbine { Id = 17, Name = "B17", Latitude = 54.61819f, Longitude = 12.66714f },
            new Turbine { Id = 18, Name = "B18", Latitude = 54.61281f, Longitude = 12.66714f },
            new Turbine { Id = 19, Name = "B19", Latitude = 54.62714f, Longitude = 12.67950f },
            new Turbine { Id = 20, Name = "B20", Latitude = 54.62175f, Longitude = 12.67950f },
            new Turbine { Id = 21, Name = "B21", Latitude = 54.63067f, Longitude = 12.69189f }
        );
	}
}
