using System;
using System.Threading;
using Bytewizer.TinyCLR.Logging;
using Bytewizer.TinyCLR.Hosting;
using Bytewizer.TinyCLR.DependencyInjection;

namespace Bytewizer.TinyCLR.DigitalPortal
{
    public delegate void ThreadPoolOptionsDelegate(ThreadPoolOptions configure);

    public static class ThreadPoolServiceCollectionExtension
    {
        public static IServiceCollection AddThreadPool(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException();
            }

            return services.AddHostedService(typeof(TheadPoolService));
        }

        public static IServiceCollection AddThreadPool(this IServiceCollection services, ThreadPoolOptionsDelegate configure)
        {
            if (services == null)
            {
                throw new ArgumentNullException();
            }

            var options = new ThreadPoolOptions();
            
            configure(options);

            ThreadPool.SetMinThreads(options.MinThreads);
            ThreadPool.SetMaxThreads(options.MaxThreads);

            return services.AddHostedService(typeof(TheadPoolService));
        }
    }
    
    public class ThreadPoolOptions 
    {
        public int MinThreads { get; set; }
        public int MaxThreads { get; set; }    
    }

    public class TheadPoolService : IHostedService
    {
        readonly ILogger _logger;
        private readonly WatchdogService _watchdog;

        public TheadPoolService(ILoggerFactory loggerFactory, WatchdogService watchdog)
        {
            _logger = loggerFactory.CreateLogger(nameof(TheadPoolService));
            _watchdog = watchdog;
        }

        public void Start()
        {
            _watchdog.RegisterService(nameof(TheadPoolService));
            ThreadPool.UnhandledThreadPoolException += ThreadUnhandledThreadPoolException;
            _watchdog.ReportHeartbeat(nameof(TheadPoolService));
        }

        public void Stop()
        {
            ThreadPool.UnhandledThreadPoolException -= ThreadUnhandledThreadPoolException;
            ThreadPool.Shutdown();
        }
        private void ThreadUnhandledThreadPoolException(object state, Exception ex)
        {
            _logger.LogError(ex, "Unhandled thread pool exception.", null);
            
            // Report critical error to watchdog
            _logger.LogCritical("Thread pool exception detected, system may be unstable");
        }
    }
}