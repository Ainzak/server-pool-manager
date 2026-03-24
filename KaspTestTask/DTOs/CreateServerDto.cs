using System.ComponentModel.DataAnnotations;

namespace KaspTestTask.DTOs
{
    public class CreateServerDto
    {
        [Required]
        public required string OperatingSystem { get; set; }

        [Range(1, int.MaxValue)]
        public int RamMb { get; set; }

        [Range(1, int.MaxValue)]
        public int DiskGb { get; set; }

        [Range(1, int.MaxValue)]
        public int CpuCores { get; set; }
    }
}
