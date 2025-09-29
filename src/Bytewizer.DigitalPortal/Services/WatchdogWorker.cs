using System;
using System.Threading;

using Bytewizer.TinyCLR.Logging;
using Bytewizer.TinyCLR.Hosting;

namespace Bytewizer.TinyCLR.DigitalPortal
{
    /// <summary>
    /// Hosted service wrapper for the watchdog to integrate with the hosting framework
    /// </summary>
    public class WatchdogWorker : IHostedService
    {
        private readonly ILogger _logger;
        private readonly WatchdogService _watchdogService;

        public WatchdogWorker(WatchdogService watchdogService, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger(nameof(WatchdogWorker));
            _watchdogService = watchdogService;
        }

        public void Start()
        {
            _logger.LogInformation("Watchdog worker started");
            
            // Register the watchdog service itself for monitoring
            _watchdogService.RegisterService(nameof(WatchdogWorker));
            _watchdogService.ReportHeartbeat(nameof(WatchdogWorker));
        }

        public void Stop()
        {
            _logger.LogInformation("Watchdog worker stopped");
            _watchdogService?.Dispose();
        }
    }
}