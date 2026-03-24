using KaspTestTask.Models;

namespace KaspTestTask.DTOs
{
    public class SearchServerDto
    {
        public ServerStatus? Status { get; set; }
        public string? OperatingSystem { get; set; }
        public int? MinRamMb { get; set; }
        public int? MinDiskGb { get; set; }
        public int? MinCpuCores { get; set; }
    }
}
