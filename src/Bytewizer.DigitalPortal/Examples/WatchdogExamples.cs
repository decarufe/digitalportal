using System;
using System.Threading;

using Bytewizer.TinyCLR.Logging;
using Bytewizer.TinyCLR.Hosting;

namespace Bytewizer.TinyCLR.DigitalPortal.Examples
{
    /// <summary>
    /// Example service demonstrating watchdog integration best practices
    /// </summary>
    public class ExampleMonitoredService : SchedulerService, IHealthCheck
    {
        private readonly ILogger _logger;
        private readonly WatchdogService _watchdog;
        private int _operationCount = 0;
        private DateTime _lastSuccessfulOperation = DateTime.Now;
        private bool _isHealthy = true;

        public ExampleMonitoredService(WatchdogService watchdog, ILoggerFactory loggerFactory)
            : base(TimeSpan.FromMinutes(1)) // Run every minute
        {
            _logger = loggerFactory.CreateLogger(nameof(ExampleMonitoredService));
            _watchdog = watchdog;
        }

        public override void Start()
        {
            try
            {
                // Register with watchdog for both heartbeat and health monitoring
                _watchdog.RegisterServiceWithHealthCheck(nameof(ExampleMonitoredService), this);
                
                _logger.LogInformation("Example service started");
                base.Start();
                
                // Report initial heartbeat
                _watchdog.ReportHeartbeat(nameof(ExampleMonitoredService));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting example service");
                _isHealthy = false;
            }
        }

        protected override void ExecuteAsync()
        {
            try
            {
                // Report heartbeat at start of operation
                _watchdog.ReportHeartbeat(nameof(ExampleMonitoredService));
                
                // Simulate some work that could potentially fail or hang
                PerformOperation();
                
                // Report successful completion
                _operationCount++;
                _lastSuccessfulOperation = DateTime.Now;
                _isHealthy = true;
                
                _logger.LogTrace("Example service operation {0} completed successfully", _operationCount);
                
                // Report heartbeat at end of operation
                _watchdog.ReportHeartbeat(nameof(ExampleMonitoredService));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in example service operation");
                _isHealthy = false;
                
                // Even if operation fails, report heartbeat to show service is still responsive
                _watchdog.ReportHeartbeat(nameof(ExampleMonitoredService));
            }
        }

        /// <summary>
        /// Simulate a potentially risky operation
        /// </summary>
        private void PerformOperation()
        {
            // Simulate network call or I/O operation with timeout protection
            var result = WatchdogExtensions.ExecuteWithTimeout(() =>
            {
                // Simulate work that could hang
                Thread.Sleep(500);
                
                // Simulate occasional failures (10% failure rate)
                if (DateTime.Now.Millisecond % 10 == 0)
                {
                    throw new InvalidOperationException("Simulated operation failure");
                }
                
                return "Success";
            }, TimeSpan.FromSeconds(2), "Timeout");

            if (result == "Timeout")
            {
                _logger.LogWarning("Operation timed out");
                _isHealthy = false;
            }
        }

        /// <summary>
        /// Implement health check to provide detailed service status
        /// </summary>
        public HealthCheckResult CheckHealth()
        {
            try
            {
                // Check if service has been successful recently
                var timeSinceSuccess = DateTime.Now - _lastSuccessfulOperation;
                if (timeSinceSuccess > TimeSpan.FromMinutes(5))
                {
                    return HealthCheckResult.Unhealthy($"No successful operations in {timeSinceSuccess.TotalMinutes:F1} minutes");
                }

                // Check basic health flag
                if (!_isHealthy)
                {
                    return HealthCheckResult.Unhealthy("Service reported unhealthy status");
                }

                // Service is healthy
                return HealthCheckResult.Healthy($"Service healthy, {_operationCount} operations completed");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("Exception during health check", ex);
            }
        }

        public override void Stop()
        {
            _logger.LogInformation("Example service stopped");
            base.Stop();
        }
    }

    /// <summary>
    /// Example of a simple service that only uses basic heartbeat monitoring
    /// </summary>
    public class SimpleMonitoredService : SchedulerService
    {
        private readonly ILogger _logger;
        private readonly WatchdogService _watchdog;

        public SimpleMonitoredService(WatchdogService watchdog, ILoggerFactory loggerFactory)
            : base(TimeSpan.FromSeconds(30)) // Run every 30 seconds
        {
            _logger = loggerFactory.CreateLogger(nameof(SimpleMonitoredService));
            _watchdog = watchdog;
        }

        public override void Start()
        {
            // Simple registration for heartbeat monitoring only
            _watchdog.RegisterService(nameof(SimpleMonitoredService));
            _logger.LogInformation("Simple service started");
            base.Start();
        }

        protected override void ExecuteAsync()
        {
            // Use the extension method for safe execution with automatic heartbeat
            _watchdog.ExecuteWithWatchdog(nameof(SimpleMonitoredService), () =>
            {
                // Your service logic here
                _logger.LogTrace("Simple service operation executed");
                
                // Simulate quick operation
                Thread.Sleep(100);
            });
        }

        public override void Stop()
        {
            _logger.LogInformation("Simple service stopped");
            base.Stop();
        }
    }
}

// Usage example in Program.cs:
/*
services.AddSingleton(typeof(ExampleMonitoredService));
services.AddSingleton(typeof(SimpleMonitoredService));

// Add as hosted services
services.AddHostedService(typeof(ExampleMonitoredService));
services.AddHostedService(typeof(SimpleMonitoredService));
*/