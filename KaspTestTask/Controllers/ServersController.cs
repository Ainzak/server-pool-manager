using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KaspTestTask.Data;
using KaspTestTask.Models;
using KaspTestTask.DTOs;

namespace KaspTestTask.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ServersController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ServersController> _logger;

        public ServersController(AppDbContext context, ILogger<ServersController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<ServerDto>> AddServer(CreateServerDto dto)
        {
            _logger.LogInformation($"Adding new server: {dto.OperatingSystem}, RAM: {dto.RamMb}, Disk: {dto.DiskGb}, CPU: {dto.CpuCores}");
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var server = new Server
            {
                Id = Guid.NewGuid(),
                OperatingSystem = dto.OperatingSystem,
                RamMb = dto.RamMb,
                DiskGb = dto.DiskGb,
                CpuCores = dto.CpuCores,
                Status = ServerStatus.Disabled
            };

            _context.Servers.Add(server);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetServer), new { id = server.Id }, MapToDto(server));
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<ServerDto>>> SearchServers([FromQuery] SearchServerDto searchDto)
        {
            _logger.LogInformation("Searching servers with parameters.");

            var query = _context.Servers.AsQueryable();

            if (searchDto.Status.HasValue)
                query = query.Where(s => s.Status == searchDto.Status.Value);

            if (!string.IsNullOrEmpty(searchDto.OperatingSystem))
                query = query.Where(s => s.OperatingSystem.Contains(searchDto.OperatingSystem));

            if (searchDto.MinRamMb.HasValue)
                query = query.Where(s => s.RamMb >= searchDto.MinRamMb.Value);

            if (searchDto.MinDiskGb.HasValue)
                query = query.Where(s => s.DiskGb >= searchDto.MinDiskGb.Value);

            if (searchDto.MinCpuCores.HasValue)
                query = query.Where(s => s.CpuCores >= searchDto.MinCpuCores.Value);

            var result = await query.ToListAsync();
            return Ok(result.Select(MapToDto));
        }

        [HttpPost("{id}/rent")]
        public async Task<IActionResult> RentServer(Guid id)
        {
            _logger.LogInformation($"Attempting to rent server {id}");

            const int maxAttempts = 3;
            int attempt = 0;
            while (true)
            {
                attempt++;
                var server = await _context.Servers.FindAsync(id);
                if (server == null)
                {
                    _logger.LogWarning($"Server {id} not found.");
                    return NotFound();
                }

                if (server.Status == ServerStatus.Reserved || server.Status == ServerStatus.Booting)
                {
                    _logger.LogWarning($"Server {id} is not available (Status: {server.Status}).");
                    return BadRequest($"Server is currently {server.Status}.");
                }

                try
                {
                    if (server.Status == ServerStatus.Available)
                    {
                        _logger.LogInformation($"Server {id} is ON. Renting immediately.");
                        server.Status = ServerStatus.Reserved;
                        server.ReservedUntil = DateTime.UtcNow.AddMinutes(20);
                        server.ReadyAt = null;
                    }
                    else if (server.Status == ServerStatus.Disabled)
                    {
                        _logger.LogInformation($"Server {id} is OFF. Starting 5-min boot process.");
                        server.Status = ServerStatus.Booting;
                        server.ReadyAt = DateTime.UtcNow.AddMinutes(5);
                    }

                    await _context.SaveChangesAsync();
                    return Ok(MapToDto(server));
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    _logger.LogWarning($"Concurrency conflict while renting server {id}: {ex.Message}");
                    if (attempt >= maxAttempts)
                    {
                        return Conflict("Another user might have rented this server simultaneously.");
                    }
                    var delayMs = 50 * (int)Math.Pow(2, attempt);
                    await Task.Delay(delayMs);
                }
            }
        }

        [HttpPost("{id}/free")]
        public async Task<IActionResult> FreeServer(Guid id)
        {
            _logger.LogInformation($"Freeing server {id}");

            const int maxAttempts = 3;
            int attempt = 0;
            while (true)
            {
                attempt++;
                var server = await _context.Servers.FindAsync(id);
                if (server == null) return NotFound();

                if (server.Status != ServerStatus.Reserved && server.Status != ServerStatus.Booting)
                {
                    return BadRequest("Server is not currently rented or booting for rent.");
                }

                try
                {
                    _logger.LogInformation($"Server {id} freed. Status: Disabled.");
                    server.Status = ServerStatus.Disabled;
                    server.ReservedUntil = null;
                    server.ReadyAt = null;

                    await _context.SaveChangesAsync();
                    return Ok(MapToDto(server));
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    _logger.LogWarning($"Concurrency conflict while freeing server {id}: {ex.Message}");
                    if (attempt >= maxAttempts)
                    {
                        return Conflict();
                    }
                    var delayMs = 50 * (int)Math.Pow(2, attempt);
                    await Task.Delay(delayMs);
                }
            }
        }

        [HttpGet("{id}/ready")]
        public async Task<ActionResult<bool>> IsReady(Guid id)
        {
            var server = await _context.Servers.FindAsync(id);
            if (server == null) return NotFound();

            if (server.Status == ServerStatus.Reserved) return Ok(true);
            if (server.Status == ServerStatus.Booting && server.ReadyAt.HasValue && server.ReadyAt.Value <= DateTime.UtcNow) return Ok(true);
            return Ok(false);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ServerDto>> GetServer(Guid id)
        {
            var server = await _context.Servers.FindAsync(id);
            if (server == null) return NotFound();
            return Ok(MapToDto(server));
        }

        private static ServerDto MapToDto(Server server)
        {
            return new ServerDto
            {
                Id = server.Id,
                OperatingSystem = server.OperatingSystem,
                RamMb = server.RamMb,
                DiskGb = server.DiskGb,
                CpuCores = server.CpuCores,
                Status = server.Status,
                ReadyAt = server.ReadyAt,
                ReservedUntil = server.ReservedUntil
            };
        }
    }
}
