using Backend.Data;
using Backend.Data.Mappers;
using Backend.Data.Repositories;
using Backend.Web.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// In deiner Program.cs – Repositories registrieren
// (Scoped passt für EF Core DbContext am besten)

//builder.Services.AddScoped<ITaskItemRepository, TaskItemRepository>();
//builder.Services.AddScoped<IPersonRepository, PersonRepository>();
//builder.Services.AddScoped<IQualificationRepository, QualificationRepository>();
//builder.Services.AddScoped<IToolRepository, ToolRepository>();
//builder.Services.AddScoped<ITaskItemRepository, TaskItemRepository>();
//builder.Services.AddScoped<IPersonRepository, PersonRepository>();
//builder.Services.AddScoped<IQualificationRepository, QualificationRepository>();
//builder.Services.AddScoped<IToolRepository, ToolRepository>();
//builder.Services.AddScoped<IPersonQualificationRepository, PersonQualificationRepository>();
//builder.Services.AddScoped<ITaskQualificationRepository, TaskQualificationRepository>();
//builder.Services.AddScoped<ITaskToolRepository, TaskToolRepository>();

// registering repositories to Dependency Injection
builder.Services.AddScoped<IRepository<TaskItem>, Repository<TaskItem>>();
builder.Services.AddScoped<IRepository<Person>, Repository<Person>>();
builder.Services.AddScoped<IRepository<Qualification>, Repository<Qualification>>();
builder.Services.AddScoped<IRepository<Tool>, Repository<Tool>>();
builder.Services.AddScoped<IRepository<TaskItem>, Repository<TaskItem>>();
//builder.Services.AddScoped<IRepository<Person>, Repository<Person>>();
//builder.Services.AddScoped<IRepository<Qualification>, Repository<Qualification>>();
//builder.Services.AddScoped<IToolRepository, ToolRepository>();
builder.Services.AddScoped<IRepository<PersonQualification>, Repository<PersonQualification>>();
builder.Services.AddScoped<IRepository<TaskQualification>, Repository<TaskQualification>>();
builder.Services.AddScoped<IRepository<TaskTool>, Repository<TaskTool>>();

//Linda meinte das geht nicht damitm sollte es gehen
builder.Services.AddScoped<PersonService>();
//builder.Services.AddScoped<GmplService>();
builder.Services.AddScoped<QualificationService>();
builder.Services.AddScoped<TaskItemService>();
builder.Services.AddScoped<ToolService>();

builder.Services.AddAutoMapper(cfg => {
    cfg.AddProfile<PersonSummaryMapper>();
    cfg.AddProfile<TaskItemSummaryMapper>();
    cfg.AddProfile<TaskItemDetailMapper>();
    cfg.AddProfile<TaskQualificationMapper>();
    cfg.AddProfile<TaskToolMapper>();
    cfg.AddProfile<ToolResponseMapper>();
    cfg.AddProfile<QualificationResponseMapper>();
});


builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>(); // ? jetzt bekannt
    db.Database.Migrate();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();