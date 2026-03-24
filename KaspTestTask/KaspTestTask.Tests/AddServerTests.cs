using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Xunit;
using Moq;
using KaspTestTask.Controllers;
using KaspTestTask.Data;
using KaspTestTask.DTOs;
using KaspTestTask.Models;

namespace KaspTestTask.Tests;

public class AddServerTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly AppDbContext _context;
    private readonly ServersController _controller;

    public AddServerTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new AppDbContext(options);
        _context.Database.EnsureCreated();

        var logger = Mock.Of<ILogger<ServersController>>();
        _controller = new ServersController(_context, logger);
    }

    [Fact]
    public async Task AddServer_WithValidData_CreatesDisabledServer()
    {
        var dto = new CreateServerDto
        {
            OperatingSystem = "Ubuntu 22.04",
            RamMb = 4096,
            DiskGb = 100,
            CpuCores = 4
        };

        var result = await _controller.AddServer(dto);

        var createdAtResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var server = Assert.IsType<ServerDto>(createdAtResult.Value);

        Assert.Equal("Ubuntu 22.04", server.OperatingSystem);
        Assert.Equal(4096, server.RamMb);
        Assert.Equal(100, server.DiskGb);
        Assert.Equal(4, server.CpuCores);
        Assert.Equal(ServerStatus.Disabled, server.Status);
        Assert.Null(server.ReadyAt);
        Assert.Null(server.ReservedUntil);
    }

    [Fact]
    public async Task AddServer_WithMinimalValues_CreatesServer()
    {
        var dto = new CreateServerDto
        {
            OperatingSystem = "Debian",
            RamMb = 1,
            DiskGb = 1,
            CpuCores = 1
        };

        var result = await _controller.AddServer(dto);

        Assert.NotNull(result.Result);
        var createdAtResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var server = Assert.IsType<ServerDto>(createdAtResult.Value);
        Assert.Equal("Debian", server.OperatingSystem);
        Assert.Equal(ServerStatus.Disabled, server.Status);
    }

    [Fact]
    public async Task AddServer_MultipleServers_CreatesAllWithUniqueIds()
    {
        var servers = new List<CreateServerDto>
        {
            new() { OperatingSystem = "Ubuntu", RamMb = 2048, DiskGb = 50, CpuCores = 2 },
            new() { OperatingSystem = "CentOS", RamMb = 4096, DiskGb = 100, CpuCores = 4 },
            new() { OperatingSystem = "Windows", RamMb = 8192, DiskGb = 200, CpuCores = 8 }
        };

        var results = new List<ServerDto>();
        foreach (var dto in servers)
        {
            var result = await _controller.AddServer(dto);
            var createdAtResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            results.Add(Assert.IsType<ServerDto>(createdAtResult.Value));
        }

        Assert.Equal(3, results.Count);
        var ids = results.Select(s => s.Id).ToList();
        Assert.Equal(3, ids.Distinct().Count());
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }
}
