using System;

using Bytewizer.TinyCLR.Logging;
using Bytewizer.TinyCLR.Hosting;
using System.Threading;

namespace Bytewizer.TinyCLR.DigitalPortal
{
    public class WeatherWorker : SchedulerService
    {
        private readonly ILogger _logger;
        private readonly WirelessService _wireless;
        private readonly WeatherService _weather;
        private readonly WatchdogService _watchdog;
       
        public WeatherWorker(WeatherService weather, WirelessService wireless, WatchdogService watchdog, ILoggerFactory loggerFactory)
            : base(TimeSpan.FromMinutes(10))
        {
            _logger = loggerFactory.CreateLogger(nameof(WeatherWorker));
            _weather = weather;
            _wireless = wireless;
            _watchdog = watchdog;
        }

        public override void Start()
        {
            _watchdog.RegisterService(nameof(WeatherWorker));
            _logger.HostStarted();

            base.Start();
        }

        protected override void ExecuteAsync()
        {
            try
            {
                _watchdog.ReportHeartbeat(nameof(WeatherWorker));
                
                if (SettingsService.NetworkConnected)
                {
                    _weather.Connect();
                }
                else
                {
                    // Instead of infinite loop, wait with timeout and retry
                    int waitAttempts = 0;
                    const int maxWaitAttempts = 30; // 30 seconds max wait
                    
                    while (!SettingsService.NetworkConnected && waitAttempts < maxWaitAttempts)
                    {
                        Thread.Sleep(1000);
                        waitAttempts++;
                        
                        // Report heartbeat every few seconds to show we're alive
                        if (waitAttempts % 5 == 0)
                        {
                            _watchdog.ReportHeartbeat(nameof(WeatherWorker));
                        }
                    }

                    if (SettingsService.NetworkConnected)
                    {
                        _weather.Connect();
                    }
                    else
                    {
                        _logger.LogWarning("Weather worker skipping update - network not available after timeout");
                    }
                }
                
                _watchdog.ReportHeartbeat(nameof(WeatherWorker));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in weather worker execution");
            }
        }

        public override void Stop()
        {
            _logger.HostStopped();

            base.Stop();
        }
    }
}