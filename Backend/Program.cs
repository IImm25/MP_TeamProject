using AutoMapper;
using Backend.Data;
using Backend.Data.Entitites;
using Backend.Data.Mappers;
using Backend.Data.Repositories;
using Backend.GMPL;
using Backend.Web.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
	options.AddPolicy("AllowAngular",
		policy =>
		{
			policy.WithOrigins("http://localhost:4200")  // Frontend-URL
				  .AllowAnyHeader()
				  .AllowAnyMethod();
		});
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// registering repositories to Dependency Injection
builder.Services.AddScoped<ITaskItemRepository, TaskItemRepository>();
builder.Services.AddScoped<IPersonRepository, PersonRepository>();
builder.Services.AddScoped<IPlanRepository, PlanRepository>();
builder.Services.AddScoped<IRepository<Qualification>, Repository<Qualification>>();
builder.Services.AddScoped<IRepository<Tool>, Repository<Tool>>();
builder.Services.AddScoped<IRepository<TaskItem>, Repository<TaskItem>>();

builder.Services.AddScoped<IRepository<PersonQualification>, Repository<PersonQualification>>();
builder.Services.AddScoped<IRepository<TaskQualification>, Repository<TaskQualification>>();
builder.Services.AddScoped<IRepository<TaskTool>, Repository<TaskTool>>();

builder.Services.AddScoped<IRepository<Location>, Repository<Location>>();

// registering services for Dependency Injection
builder.Services.AddScoped<PersonService>();
builder.Services.AddScoped<QualificationService>();
builder.Services.AddScoped<TaskItemService>();
builder.Services.AddScoped<ToolService>();
builder.Services.AddScoped<PlanService>();
builder.Services.AddScoped<LocationService>();

// register singletons
builder.Services.AddSingleton<IGlpkSolver,GlpkSolver>();
builder.Services.AddSingleton<IGmplInputBuilder,GmplInputBuilder>();


builder.Services.AddAutoMapper(cfg => {
    cfg.AddProfile<PersonSummaryMapper>();
    cfg.AddProfile<TaskItemSummaryMapper>();
    cfg.AddProfile<TaskItemDetailMapper>();
    cfg.AddProfile<TaskQualificationMapper>();
    cfg.AddProfile<TaskToolMapper>();
    cfg.AddProfile<ToolResponseMapper>();
    cfg.AddProfile<QualificationResponseMapper>();
    cfg.AddProfile<PersonDetailMapper>();
    cfg.AddProfile<PersonQualificationMapper>();
    cfg.AddProfile<LocationMapper>();
    cfg.AddProfile<SingleTaskScheduleMapper>();
    cfg.AddProfile<PlanMapper>();
});


builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

var mapper = app.Services.GetRequiredService<IMapper>();
mapper.ConfigurationProvider.AssertConfigurationIsValid();


app.UseCors("AllowAngular");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();

public partial class Program { }