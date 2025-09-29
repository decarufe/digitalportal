# Watchdog System for TinyCLR Digital Portal

## Overview

The Watchdog System provides comprehensive monitoring and reliability features for the TinyCLR-based Digital Portal application. It prevents system crashes, detects unresponsive services, and automatically recovers from various failure scenarios.

## Features

### Core Functionality
- **Service Health Monitoring**: Tracks heartbeats from all registered services
- **Memory Pressure Detection**: Monitors available memory and triggers garbage collection
- **Network Health Checks**: Validates network connectivity and IP configuration
- **Automatic Recovery**: Performs graduated recovery actions based on failure severity
- **Comprehensive Logging**: Detailed system statistics and error reporting

### Safety Mechanisms
- **Timeout Detection**: Services must report within configured intervals (default: 5 minutes)
- **Consecutive Failure Tracking**: Progressive recovery based on failure count
- **Resource Monitoring**: Memory allocation testing and cleanup
- **Thread Safety**: All operations are thread-safe for multi-threaded environments

## Configuration

### Basic Usage

```csharp
// In Program.cs - ConfigureServices
services.AddWatchdogForDevelopment(); // For development/testing
// OR
services.AddWatchdogForProduction();  // For production deployment
```

### Custom Configuration

```csharp
services.AddWatchdog(options => {
    options.WatchdogInterval = TimeSpan.FromSeconds(15);        // Check every 15 seconds
    options.HealthCheckInterval = TimeSpan.FromMinutes(1);      // Deep checks every minute
    options.MaxServiceTimeout = TimeSpan.FromMinutes(3);       // Service timeout
    options.MaxConsecutiveFailures = 2;                        // Recovery threshold
    options.EnableSystemReset = true;                          // Allow full system reset
    options.VerboseLogging = true;                              // Debug logging
});
```

### Environment Presets

| Environment | Watchdog Interval | Health Check | Service Timeout | Max Failures | System Reset |
|-------------|-------------------|--------------|-----------------|--------------|--------------|
| Development | 15 seconds        | 45 seconds   | 2 minutes       | 2            | Disabled     |
| Default     | 30 seconds        | 2 minutes    | 5 minutes       | 3            | Disabled     |
| Production  | 30 seconds        | 5 minutes    | 10 minutes      | 3            | Enabled      |

## Service Integration

### Basic Service Registration

```csharp
public class MyWorker : SchedulerService
{
    private readonly WatchdogService _watchdog;

    public MyWorker(WatchdogService watchdog, ILoggerFactory loggerFactory)
        : base(TimeSpan.FromMinutes(1))
    {
        _watchdog = watchdog;
    }

    public override void Start()
    {
        _watchdog.RegisterService(nameof(MyWorker));
        base.Start();
    }

    protected override void ExecuteAsync()
    {
        try
        {
            _watchdog.ReportHeartbeat(nameof(MyWorker));
            
            // Your service logic here
            
            _watchdog.ReportHeartbeat(nameof(MyWorker));
        }
        catch (Exception ex)
        {
            // Handle errors but still report heartbeat
            _watchdog.ReportHeartbeat(nameof(MyWorker));
            throw;
        }
    }
}
```

### Advanced Health Check Integration

```csharp
public class MyService : IHealthCheck
{
    public HealthCheckResult CheckHealth()
    {
        try
        {
            // Perform service-specific health validation
            if (IsOperational())
                return HealthCheckResult.Healthy("Service is operational");
            else
                return HealthCheckResult.Unhealthy("Service not responding");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Health check failed", ex);
        }
    }
}

// Register with health checking
_watchdog.RegisterServiceWithHealthCheck("MyService", myServiceInstance);
```

## Recovery Actions

The watchdog performs graduated recovery based on failure severity:

### Level 1: Memory Cleanup
- Force garbage collection
- Clear managed resources
- Log memory statistics

### Level 2: Service Recovery
- Report service timeouts
- Allow service-specific recovery
- Continue monitoring

### Level 3: System Reset (Production Only)
- Triggered after consecutive failures exceed threshold
- Logs critical error before reset
- Requires `EnableSystemReset = true`

## Monitoring and Diagnostics

### Log Output Examples

```
[INFO] Watchdog service started with 30s interval
[TRACE] Service registered for monitoring: WirelessWorker
[TRACE] Service with health check registered: WirelessService
[INFO] System Stats - Memory: 45312 bytes, Uptime: 15.2 minutes, Monitored Services: 4
[WARNING] Service WeatherWorker timeout detected: 6.2 minutes since last heartbeat
[ERROR] System health check failed. Consecutive failures: 2
[CRITICAL] Triggering system recovery due to persistent health issues
```

### Health Check Results

Services implementing `IHealthCheck` provide detailed status:

```
[TRACE] Health check passed for WirelessService: Network connected with IP: 192.168.1.100
[WARNING] Health check failed for WeatherService: API timeout exceeded
```

## Best Practices

### Service Implementation
1. **Report heartbeats regularly** - At start and end of operations
2. **Handle exceptions gracefully** - Don't let errors prevent heartbeat reporting
3. **Implement health checks** - For critical services that can be validated
4. **Use timeouts** - For all external operations (HTTP, I/O)

### Configuration Guidelines
1. **Development**: Use shorter intervals for faster feedback
2. **Production**: Use longer intervals to reduce overhead
3. **Embedded Devices**: Consider memory constraints in test allocation size
4. **Critical Systems**: Enable system reset for maximum reliability

### Error Handling
1. **Never block watchdog threads** - Keep operations lightweight
2. **Log but don't throw** - Let the watchdog continue monitoring
3. **Graceful degradation** - Continue operation with reduced functionality
4. **Resource cleanup** - Dispose resources properly in recovery scenarios

## Troubleshooting

### Common Issues

**Service Timeouts**
- Check if service is reporting heartbeats regularly
- Verify service isn't blocked in long-running operations
- Increase timeout if service legitimately needs more time

**Memory Pressure**
- Reduce memory test allocation size
- Check for memory leaks in application code
- Force more frequent garbage collection

**False Recoveries**
- Increase consecutive failure threshold
- Extend service timeout periods
- Review service health check logic

**System Not Recovering**
- Verify recovery actions are appropriate
- Check if system reset is enabled in production
- Review log output for specific failure causes

## Performance Considerations

### Memory Usage
- Watchdog uses minimal memory (hashtables for service tracking)
- Memory test allocation is configurable (default: 1KB)
- Automatic garbage collection helps maintain available memory

### CPU Overhead
- Timer-based checks minimize CPU usage
- Health checks run on separate schedule
- Configurable intervals allow tuning for performance

### Thread Safety
- All watchdog operations are thread-safe
- Uses locking only for critical sections
- Doesn't block application threads

## Migration Guide

### From Legacy Code

The new watchdog system replaces manual timer-based monitoring:

```csharp
// Old approach
Timer timer = new Timer(TimerTick, null, 3600000, 3600000);

// New approach
services.AddWatchdogForProduction();
```

### Integrating Existing Services

1. Add watchdog parameter to constructor
2. Register service in Start() method
3. Report heartbeat in main execution loop
4. Optionally implement IHealthCheck for advanced monitoring

This comprehensive watchdog system significantly improves the reliability and robustness of the TinyCLR Digital Portal application, making it suitable for production deployment on embedded devices.