using System;

namespace Bytewizer.TinyCLR.DigitalPortal
{
    /// <summary>
    /// Health check result for service monitoring
    /// </summary>
    public class HealthCheckResult
    {
        public bool IsHealthy { get; set; }
        public string Message { get; set; }
        public Exception Exception { get; set; }

        public static HealthCheckResult Healthy(string message = "Service is healthy")
        {
            return new HealthCheckResult { IsHealthy = true, Message = message };
        }

        public static HealthCheckResult Unhealthy(string message, Exception exception = null)
        {
            return new HealthCheckResult { IsHealthy = false, Message = message, Exception = exception };
        }
    }

    /// <summary>
    /// Interface for services that can perform health checks
    /// </summary>
    public interface IHealthCheck
    {
        HealthCheckResult CheckHealth();
    }

    /// <summary>
    /// Extension methods for watchdog integration
    /// </summary>
    public static class WatchdogExtensions
    {
        /// <summary>
        /// Safely execute an action and report to watchdog
        /// </summary>
        public static void ExecuteWithWatchdog(this WatchdogService watchdog, string serviceName, Action action)
        {
            try
            {
                action();
                watchdog.ReportHeartbeat(serviceName);
            }
            catch (Exception ex)
            {
                // Log error and still report heartbeat to prevent false alarms
                System.Diagnostics.Debug.WriteLine($"Error in {serviceName}: {ex.Message}");
                watchdog.ReportHeartbeat(serviceName);
                throw; // Re-throw to maintain original behavior
            }
        }

        /// <summary>
        /// Execute function with timeout protection
        /// </summary>
        public static T ExecuteWithTimeout<T>(Func<T> func, TimeSpan timeout, T defaultValue = default(T))
        {
            try
            {
                // Simple timeout simulation - in a real embedded system you might use a Timer
                var startTime = DateTime.Now;
                var result = func();
                var elapsed = DateTime.Now - startTime;
                
                if (elapsed > timeout)
                {
                    System.Diagnostics.Debug.WriteLine($"Operation took {elapsed.TotalMilliseconds}ms (timeout was {timeout.TotalMilliseconds}ms)");
                }
                
                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Operation failed: {ex.Message}");
                return defaultValue;
            }
        }
    }
}