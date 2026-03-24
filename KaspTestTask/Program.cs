using KaspTestTask.Data;
using KaspTestTask.Models;
using KaspTestTask.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=servers.db"));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHostedService<ServerManagerService>();

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    
        await db.Database.MigrateAsync();

        if (!db.Servers.Any())
    {
        db.Servers.AddRange(new List<Server>
        {
            new Server { Id = Guid.NewGuid(), OperatingSystem = "Ubuntu 22.04", RamMb = 8192, DiskGb = 100, CpuCores = 4, Status = ServerStatus.Available },
            new Server { Id = Guid.NewGuid(), OperatingSystem = "Windows Server 2022", RamMb = 16384, DiskGb = 200, CpuCores = 8, Status = ServerStatus.Disabled },
            new Server { Id = Guid.NewGuid(), OperatingSystem = "CentOS 7", RamMb = 4096, DiskGb = 50, CpuCores = 2, Status = ServerStatus.Available }
        });
        await db.SaveChangesAsync();
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
