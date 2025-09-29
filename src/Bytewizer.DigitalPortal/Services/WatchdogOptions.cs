using System;

namespace Bytewizer.TinyCLR.DigitalPortal
{
    /// <summary>
    /// Configuration options for the watchdog service
    /// </summary>
    public class WatchdogOptions
    {
        /// <summary>
        /// How often to check for system health (default: 30 seconds)
        /// </summary>
        public TimeSpan WatchdogInterval { get; set; } = TimeSpan.FromSeconds(30);

        /// <summary>
        /// How often to perform comprehensive health checks (default: 2 minutes)
        /// </summary>
        public TimeSpan HealthCheckInterval { get; set; } = TimeSpan.FromMinutes(2);

        /// <summary>
        /// Maximum time a service can go without reporting (default: 5 minutes)
        /// </summary>
        public TimeSpan MaxServiceTimeout { get; set; } = TimeSpan.FromMinutes(5);

        /// <summary>
        /// Number of consecutive failures before triggering recovery (default: 3)
        /// </summary>
        public int MaxConsecutiveFailures { get; set; } = 3;

        /// <summary>
        /// Size of test memory allocation for memory pressure detection (default: 1024 bytes)
        /// </summary>
        public int MemoryTestSize { get; set; } = 1024;

        /// <summary>
        /// Whether to enable automatic system reset as last resort (default: false)
        /// </summary>
        public bool EnableSystemReset { get; set; } = false;

        /// <summary>
        /// Timeout for HTTP operations (default: 10 seconds)
        /// </summary>
        public TimeSpan HttpTimeout { get; set; } = TimeSpan.FromSeconds(10);

        /// <summary>
        /// Enable verbose logging for debugging (default: false)
        /// </summary>
        public bool VerboseLogging { get; set; } = false;

        /// <summary>
        /// Validate configuration and set reasonable defaults
        /// </summary>
        public void Validate()
        {
            if (WatchdogInterval.TotalSeconds < 10)
                WatchdogInterval = TimeSpan.FromSeconds(10);

            if (HealthCheckInterval < WatchdogInterval)
                HealthCheckInterval = TimeSpan.FromMilliseconds(WatchdogInterval.TotalMilliseconds * 2);

            if (MaxServiceTimeout < HealthCheckInterval)
                MaxServiceTimeout = TimeSpan.FromMilliseconds(HealthCheckInterval.TotalMilliseconds * 2);

            if (MaxConsecutiveFailures < 1)
                MaxConsecutiveFailures = 1;

            if (MemoryTestSize < 512)
                MemoryTestSize = 512;

            if (HttpTimeout.TotalSeconds < 2)
                HttpTimeout = TimeSpan.FromSeconds(2);
        }

        /// <summary>
        /// Create default configuration optimized for embedded devices
        /// </summary>
        public static WatchdogOptions CreateDefault()
        {
            var options = new WatchdogOptions();
            options.Validate();
            return options;
        }

        /// <summary>
        /// Create configuration for development/testing with shorter intervals
        /// </summary>
        public static WatchdogOptions CreateDevelopment()
        {
            return new WatchdogOptions
            {
                WatchdogInterval = TimeSpan.FromSeconds(15),
                HealthCheckInterval = TimeSpan.FromSeconds(45),
                MaxServiceTimeout = TimeSpan.FromMinutes(2),
                MaxConsecutiveFailures = 2,
                VerboseLogging = true,
                EnableSystemReset = false
            };
        }

        /// <summary>
        /// Create configuration for production with aggressive recovery
        /// </summary>
        public static WatchdogOptions CreateProduction()
        {
            return new WatchdogOptions
            {
                WatchdogInterval = TimeSpan.FromSeconds(30),
                HealthCheckInterval = TimeSpan.FromMinutes(5),
                MaxServiceTimeout = TimeSpan.FromMinutes(10),
                MaxConsecutiveFailures = 3,
                VerboseLogging = false,
                EnableSystemReset = true
            };
        }
    }
}