using System;

using Bytewizer.TinyCLR.Logging;
using Bytewizer.TinyCLR.Hosting;
using System.Threading;

namespace Bytewizer.TinyCLR.DigitalPortal
{
    public class DisplayWorker : SchedulerService
    {
        private readonly ILogger _logger;
        private readonly MainWindow _mainWindow;
        private readonly WatchdogService _watchdog;
        private int _state;

       
        public DisplayWorker(MainWindow mainWindow, WatchdogService watchdog, ILoggerFactory loggerFactory)
            : base(0,1,TimeSpan.FromSeconds(30))
        {
            _logger = loggerFactory.CreateLogger(nameof(DisplayWorker));
            _mainWindow = mainWindow;
            _watchdog = watchdog;
        }

        public override void Start()
        {
            _watchdog.RegisterService(nameof(DisplayWorker));
            _logger.HostStarted();

            base.Start();
        }

        protected override void ExecuteAsync()
        {
            try
            {
                _watchdog.ReportHeartbeat(nameof(DisplayWorker));
                
                if (_state == 0)
                {
                    _state = 1;
                    _mainWindow.Activate(1, true);
                }
                else
                {
                    _state = 0;
                    _mainWindow.Activate(0, true);
                }
                
                _watchdog.ReportHeartbeat(nameof(DisplayWorker));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in display worker execution");
            }
        }

        public override void Stop()
        {
            _logger.HostStopped();

            base.Stop();
        }
    }
}