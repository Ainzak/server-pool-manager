using KaspTestTask.Models;

namespace KaspTestTask.DTOs
{
    public class ServerDto
    {
        public Guid Id { get; set; }
        public required string OperatingSystem { get; set; }
        public int RamMb { get; set; }
        public int DiskGb { get; set; }
        public int CpuCores { get; set; }
        public ServerStatus Status { get; set; }
        public DateTime? ReadyAt { get; set; }
        public DateTime? ReservedUntil { get; set; }
    }
}
