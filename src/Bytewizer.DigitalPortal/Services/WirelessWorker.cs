using System;

using Bytewizer.TinyCLR.Logging;
using Bytewizer.TinyCLR.Hosting;

using GHIElectronics.TinyCLR.Devices.Network;

namespace Bytewizer.TinyCLR.DigitalPortal
{
    public class WirelessWorker : SchedulerService
    {
        private readonly ILogger _logger;
        private readonly WirelessService _network;
        private readonly WatchdogService _watchdog;

        public WirelessWorker(WirelessService network, WatchdogService watchdog, ILoggerFactory loggerFactory)
            : base(TimeSpan.FromMinutes(1))
        {
            _logger = loggerFactory.CreateLogger(nameof(WirelessWorker));          
            _network = network;
            _watchdog = watchdog;
        }

        protected override void ExecuteAsync()
        {
            try
            {
                _watchdog.ReportHeartbeat(nameof(WirelessWorker));
                
                if (SettingsService.NetworkConnected)
                {
                    return;
                }

                _network.Disable();
                _network.Enable();
                
                _watchdog.ReportHeartbeat(nameof(WirelessWorker));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in wireless worker execution");
            }
        }

        public override void Start()
        {
            try
            {
                _watchdog.RegisterServiceWithHealthCheck(nameof(WirelessWorker), _network);
                
                if (SettingsService.NetworkConnected)
                {
                    return;
                }

                var flash = SettingsService.Flash;
                _network.Controller.SetInterfaceSettings(new WiFiNetworkInterfaceSettings()
                {
                    Ssid = flash.Ssid,
                    Password = flash.Password,
                });

                _network.Enable();
                _logger.HostStarted();

                base.Start();
                
                _watchdog.ReportHeartbeat(nameof(WirelessWorker));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting wireless worker");
            }
        }

        public override void Stop()
        {
            _network.Disable();
            _logger.HostStopped();
            
            base.Stop();
        }
    }
}
