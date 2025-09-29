using System;
using System.Threading;
using System.Collections;

using Bytewizer.TinyCLR.Logging;
using Bytewizer.TinyCLR.DependencyInjection;

using GHIElectronics.TinyCLR.Native;

namespace Bytewizer.TinyCLR.DigitalPortal
{
    /// <summary>
    /// Watchdog service to monitor system health and prevent crashes
    /// </summary>
    public class WatchdogService : IDisposable
    {
        private readonly ILogger _logger;
        private readonly Timer _watchdogTimer;
        private readonly Timer _healthCheckTimer;
        private readonly Hashtable _serviceHeartbeats;
        private readonly Hashtable _healthCheckServices;
        private readonly object _lockObject = new object();
        
        private bool _isDisposed = false;
        private DateTime _lastSystemCheck;
        private int _consecutiveFailures = 0;
        
        // Configuration
        private readonly TimeSpan _watchdogInterval = TimeSpan.FromSeconds(30);
        private readonly TimeSpan _healthCheckInterval = TimeSpan.FromMinutes(2);
        private readonly TimeSpan _maxServiceTimeout = TimeSpan.FromMinutes(5);
        private readonly int _maxConsecutiveFailures = 3;

        public WatchdogService(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger(nameof(WatchdogService));
            _serviceHeartbeats = new Hashtable();
            _healthCheckServices = new Hashtable();
            _lastSystemCheck = DateTime.Now;
            
            // Initialize timers
            _watchdogTimer = new Timer(WatchdogCheck, null, _watchdogInterval, _watchdogInterval);
            _healthCheckTimer = new Timer(HealthCheck, null, _healthCheckInterval, _healthCheckInterval);
            
            _logger.LogInformation("Watchdog service started with {0}s interval", _watchdogInterval.TotalSeconds);
        }

        /// <summary>
        /// Register a service for monitoring
        /// </summary>
        public void RegisterService(string serviceName)
        {
            lock (_lockObject)
            {
                _serviceHeartbeats[serviceName] = DateTime.Now;
                _logger.LogTrace("Service registered for monitoring: {0}", serviceName);
            }
        }

        /// <summary>
        /// Register a service with health check capability
        /// </summary>
        public void RegisterServiceWithHealthCheck(string serviceName, IHealthCheck healthCheck)
        {
            lock (_lockObject)
            {
                _serviceHeartbeats[serviceName] = DateTime.Now;
                _healthCheckServices[serviceName] = healthCheck;
                _logger.LogTrace("Service with health check registered: {0}", serviceName);
            }
        }

        /// <summary>
        /// Report service heartbeat
        /// </summary>
        public void ReportHeartbeat(string serviceName)
        {
            lock (_lockObject)
            {
                _serviceHeartbeats[serviceName] = DateTime.Now;
            }
        }

        /// <summary>
        /// Main watchdog check routine
        /// </summary>
        private void WatchdogCheck(object state)
        {
            try
            {
                var currentTime = DateTime.Now;
                bool systemHealthy = true;

                // Check if services are responding
                lock (_lockObject)
                {
                    foreach (DictionaryEntry entry in _serviceHeartbeats)
                    {
                        var serviceName = (string)entry.Key;
                        var lastHeartbeat = (DateTime)entry.Value;
                        var timeSinceHeartbeat = currentTime - lastHeartbeat;

                        if (timeSinceHeartbeat > _maxServiceTimeout)
                        {
                            _logger.LogWarning("Service {0} timeout detected: {1} minutes since last heartbeat", 
                                serviceName, timeSinceHeartbeat.TotalMinutes);
                            systemHealthy = false;
                        }
                    }
                }

                // Check memory pressure
                if (!CheckMemoryHealth())
                {
                    systemHealthy = false;
                }

                // Update system status
                if (systemHealthy)
                {
                    _consecutiveFailures = 0;
                    _lastSystemCheck = currentTime;
                }
                else
                {
                    _consecutiveFailures++;
                    _logger.LogError("System health check failed. Consecutive failures: {0}", _consecutiveFailures);

                    if (_consecutiveFailures >= _maxConsecutiveFailures)
                    {
                        TriggerRecovery();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in watchdog check");
            }
        }

        /// <summary>
        /// Perform comprehensive health checks
        /// </summary>
        private void HealthCheck(object state)
        {
            try
            {
                _logger.LogTrace("Performing system health check");
                
                // Perform health checks on registered services
                lock (_lockObject)
                {
                    foreach (DictionaryEntry entry in _healthCheckServices)
                    {
                        var serviceName = (string)entry.Key;
                        var healthCheck = (IHealthCheck)entry.Value;
                        
                        try
                        {
                            var result = healthCheck.CheckHealth();
                            if (!result.IsHealthy)
                            {
                                _logger.LogWarning("Health check failed for {0}: {1}", serviceName, result.Message);
                            }
                            else
                            {
                                _logger.LogTrace("Health check passed for {0}: {1}", serviceName, result.Message);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Exception during health check for {0}", serviceName);
                        }
                    }
                }
                
                // Log system statistics
                LogSystemStatistics();
                
                // Reset heartbeat for system check
                ReportHeartbeat("SystemCheck");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in health check");
            }
        }

        /// <summary>
        /// Check memory health and availability
        /// </summary>
        private bool CheckMemoryHealth()
        {
            try
            {
                // Force garbage collection to get accurate memory reading
                GC.WaitForPendingFinalizers();
                GC.Collect();

                var totalMemory = GC.GetTotalMemory(false);
                
                // Log memory usage periodically
                if (_consecutiveFailures == 0) // Only log when healthy to reduce noise
                {
                    _logger.LogTrace("Current memory usage: {0} bytes", totalMemory);
                }

                // Simple memory pressure check - if we can't allocate a reasonable buffer, we're in trouble
                try
                {
                    var testBuffer = new byte[1024]; // 1KB test allocation
                    return true;
                }
                catch (OutOfMemoryException)
                {
                    _logger.LogError("Memory pressure detected - unable to allocate test buffer");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking memory health");
                return false;
            }
        }

        /// <summary>
        /// Log system statistics for monitoring
        /// </summary>
        private void LogSystemStatistics()
        {
            try
            {
                var totalMemory = GC.GetTotalMemory(false);
                var uptime = DateTime.Now - _lastSystemCheck;
                
                _logger.LogInformation("System Stats - Memory: {0} bytes, Uptime: {1} minutes, Monitored Services: {2}", 
                    totalMemory, uptime.TotalMinutes, _serviceHeartbeats.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging system statistics");
            }
        }

        /// <summary>
        /// Trigger recovery actions when system is unhealthy
        /// </summary>
        private void TriggerRecovery()
        {
            try
            {
                _logger.LogCritical("Triggering system recovery due to persistent health issues");
                
                // First attempt: Force garbage collection and clear resources
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                
                _logger.LogWarning("Forced garbage collection completed");
                
                // If we still have issues, we could implement more aggressive recovery
                // For now, just reset the failure counter and continue monitoring
                Thread.Sleep(5000); // Give system time to recover
                _consecutiveFailures = 0;
                
                // In a production system, you might want to:
                // 1. Restart specific services
                // 2. Reset hardware components
                // 3. Perform a system restart as last resort
                // Power.Reset(); // Uncomment for full system reset
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during recovery actions");
            }
        }

        public void Dispose()
        {
            if (!_isDisposed)
            {
                _watchdogTimer?.Dispose();
                _healthCheckTimer?.Dispose();
                _logger.LogInformation("Watchdog service disposed");
                _isDisposed = true;
            }
        }
    }
}