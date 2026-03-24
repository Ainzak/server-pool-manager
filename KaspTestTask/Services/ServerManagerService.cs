using KaspTestTask.Data;
using KaspTestTask.Models;
using Microsoft.EntityFrameworkCore;

namespace KaspTestTask.Services
{
    public class ServerManagerService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ServerManagerService> _logger;

        public ServerManagerService(IServiceProvider serviceProvider, ILogger<ServerManagerService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("ServerManagerService is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    var now = DateTime.UtcNow;

                    var bootingServers = await db.Servers
    .Where(s => s.Status == ServerStatus.Booting && s.ReadyAt != null && s.ReadyAt <= now)
    .ToListAsync(stoppingToken);

                    foreach (var server in bootingServers)
                    {
                        _logger.LogInformation($"Server {server.Id} finished booting. Status: Reserved.");
                        server.Status = ServerStatus.Reserved;
                        server.ReservedUntil = now.AddMinutes(20);
                        server.ReadyAt = null;
                    }

                    var expiredReservations = await db.Servers
    .Where(s => s.Status == ServerStatus.Reserved && s.ReservedUntil != null && s.ReservedUntil <= now)
    .ToListAsync(stoppingToken);

                    foreach (var server in expiredReservations)
                    {
                        _logger.LogInformation($"Server {server.Id} reservation expired. Shutting down (Disabled).");
                        server.Status = ServerStatus.Disabled;
                        server.ReservedUntil = null;
                        server.ReadyAt = null;
                    }

                    if (bootingServers.Any() || expiredReservations.Any())
                    {
                        const int maxAttempts = 3;
                        int attempt = 0;
                        while (true)
                        {
                            attempt++;
                            try
                            {
                                await db.SaveChangesAsync(stoppingToken);
                                break;
                            }
                            catch (DbUpdateConcurrencyException ex)
                            {
                                _logger.LogWarning($"Concurrency conflict during background status update (attempt {attempt}): {ex.Message}");
                                if (attempt >= maxAttempts)
                                {
                                    _logger.LogError("Failed to update server statuses after retries.");
                                    break;
                                }
                                var delayMs = 50 * (int)Math.Pow(2, attempt);
                                await Task.Delay(delayMs, stoppingToken);
                            }
                        }
                    }
                }

                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }

            _logger.LogInformation("ServerManagerService is stopping.");
        }
    }
}
