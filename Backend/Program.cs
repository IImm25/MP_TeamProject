using Backend.Data;
using Backend.Data.Mappers;
using Backend.Data.Repositories;
using Backend.Web.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// registering repositories to Dependency Injection
builder.Services.AddScoped<IRepository<TaskItem>, Repository<TaskItem>>();
builder.Services.AddScoped<IRepository<Person>, Repository<Person>>();
builder.Services.AddScoped<IRepository<Qualification>, Repository<Qualification>>();
builder.Services.AddScoped<IRepository<Tool>, Repository<Tool>>();
builder.Services.AddScoped<IRepository<TaskItem>, Repository<TaskItem>>();

builder.Services.AddScoped<IRepository<PersonQualification>, Repository<PersonQualification>>();
builder.Services.AddScoped<IRepository<TaskQualification>, Repository<TaskQualification>>();
builder.Services.AddScoped<IRepository<TaskTool>, Repository<TaskTool>>();

// registering services for Dependency Injection
builder.Services.AddScoped<PersonService>();
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
    cfg.AddProfile<PersonDetailMapper>();
    cfg.AddProfile<PersonQualificationMapper>();
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
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();