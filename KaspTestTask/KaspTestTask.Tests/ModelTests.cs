using Xunit;
using KaspTestTask.Models;
using KaspTestTask.DTOs;

namespace KaspTestTask.Tests;

public class ModelTests
{
    [Fact]
    public void ServerStatus_Enum_HasExpectedValues()
    {
        Assert.Equal(0, (int)ServerStatus.Available);
        Assert.Equal(1, (int)ServerStatus.Reserved);
        Assert.Equal(2, (int)ServerStatus.Booting);
        Assert.Equal(3, (int)ServerStatus.Disabled);
    }

    [Fact]
    public void CreateServerDto_CanBeCreated()
    {
        var dto = new CreateServerDto
        {
            OperatingSystem = "Ubuntu",
            RamMb = 4096,
            DiskGb = 100,
            CpuCores = 4
        };

        Assert.Equal("Ubuntu", dto.OperatingSystem);
        Assert.Equal(4096, dto.RamMb);
        Assert.Equal(100, dto.DiskGb);
        Assert.Equal(4, dto.CpuCores);
    }

    [Fact]
    public void ServerDto_CanBeCreated()
    {
        var dto = new ServerDto
        {
            Id = Guid.NewGuid(),
            OperatingSystem = "CentOS",
            RamMb = 2048,
            DiskGb = 50,
            CpuCores = 2,
            Status = ServerStatus.Available
        };

        Assert.NotEqual(Guid.Empty, dto.Id);
        Assert.Equal("CentOS", dto.OperatingSystem);
        Assert.Equal(ServerStatus.Available, dto.Status);
    }
}
