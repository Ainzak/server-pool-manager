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

public class SearchServerTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly AppDbContext _context;
    private readonly ServersController _controller;

    public SearchServerTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new AppDbContext(options);
        _context.Database.EnsureCreated();
        
        SeedTestData();
        
        var logger = Mock.Of<ILogger<ServersController>>();
        _controller = new ServersController(_context, logger);
    }

    private void SeedTestData()
    {
        _context.Servers.AddRange(
            new Server { Id = Guid.NewGuid(), OperatingSystem = "Ubuntu 22.04", RamMb = 8192, DiskGb = 100, CpuCores = 4, Status = ServerStatus.Available },
            new Server { Id = Guid.NewGuid(), OperatingSystem = "Ubuntu 20.04", RamMb = 4096, DiskGb = 50, CpuCores = 2, Status = ServerStatus.Available },
            new Server { Id = Guid.NewGuid(), OperatingSystem = "Windows Server 2022", RamMb = 16384, DiskGb = 200, CpuCores = 8, Status = ServerStatus.Disabled },
            new Server { Id = Guid.NewGuid(), OperatingSystem = "CentOS 7", RamMb = 2048, DiskGb = 50, CpuCores = 2, Status = ServerStatus.Reserved },
            new Server { Id = Guid.NewGuid(), OperatingSystem = "Debian 11", RamMb = 1024, DiskGb = 25, CpuCores = 1, Status = ServerStatus.Available }
        );
        _context.SaveChanges();
    }

    [Fact]
    public async Task SearchServers_WithoutFilters_ReturnsAllServers()
    {
        var searchDto = new SearchServerDto();

        var result = await _controller.SearchServers(searchDto);
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var servers = Assert.IsAssignableFrom<IEnumerable<ServerDto>>(okResult.Value).ToList();

        Assert.Equal(5, servers.Count);
    }

    [Fact]
    public async Task SearchServers_ByStatusAvailable_ReturnsOnlyAvailable()
    {
        var searchDto = new SearchServerDto { Status = ServerStatus.Available };

        var result = await _controller.SearchServers(searchDto);
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var servers = Assert.IsAssignableFrom<IEnumerable<ServerDto>>(okResult.Value).ToList();

        Assert.Equal(3, servers.Count);
        Assert.All(servers, s => Assert.Equal(ServerStatus.Available, s.Status));
    }

    [Fact]
    public async Task SearchServers_ByOperatingSystem_ReturnsMatchingServers()
    {
        var searchDto = new SearchServerDto { OperatingSystem = "Ubuntu" };

        var result = await _controller.SearchServers(searchDto);
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var servers = Assert.IsAssignableFrom<IEnumerable<ServerDto>>(okResult.Value).ToList();

        Assert.Equal(2, servers.Count);
        Assert.All(servers, s => Assert.Contains("Ubuntu", s.OperatingSystem));
    }

    [Fact]
    public async Task SearchServers_ByMinRam_ReturnsServersWithEnoughRam()
    {
        var searchDto = new SearchServerDto { MinRamMb = 4096 };

        var result = await _controller.SearchServers(searchDto);
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var servers = Assert.IsAssignableFrom<IEnumerable<ServerDto>>(okResult.Value).ToList();

        Assert.Equal(3, servers.Count);
        Assert.All(servers, s => Assert.True(s.RamMb >= 4096));
    }

    [Fact]
    public async Task SearchServers_WithMultipleFilters_ReturnsMatchingServers()
    {
        var searchDto = new SearchServerDto 
        { 
            Status = ServerStatus.Available,
            MinRamMb = 4096,
            MinCpuCores = 2
        };

        var result = await _controller.SearchServers(searchDto);
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var servers = Assert.IsAssignableFrom<IEnumerable<ServerDto>>(okResult.Value).ToList();

        Assert.Equal(2, servers.Count);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }
}
