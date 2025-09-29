using System;
using Bytewizer.TinyCLR.DependencyInjection;

namespace Bytewizer.TinyCLR.DigitalPortal
{
    /// <summary>
    /// Extension methods for adding watchdog services to the service collection
    /// </summary>
    public static class WatchdogServiceCollectionExtensions
    {
        /// <summary>
        /// Add watchdog services with default configuration
        /// </summary>
        public static IServiceCollection AddWatchdog(this IServiceCollection services)
        {
            return AddWatchdog(services, WatchdogOptions.CreateDefault());
        }

        /// <summary>
        /// Add watchdog services with custom configuration
        /// </summary>
        public static IServiceCollection AddWatchdog(this IServiceCollection services, WatchdogOptions options)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (options == null)
                throw new ArgumentNullException(nameof(options));

            options.Validate();

            // Register configuration
            services.AddSingleton(typeof(WatchdogOptions), options);

            // Register watchdog services
            services.AddSingleton(typeof(WatchdogService));
            services.AddHostedService(typeof(WatchdogWorker));

            return services;
        }

        /// <summary>
        /// Add watchdog services with configuration delegate
        /// </summary>
        public static IServiceCollection AddWatchdog(this IServiceCollection services, Action<WatchdogOptions> configureOptions)
        {
            if (configureOptions == null)
                throw new ArgumentNullException(nameof(configureOptions));

            var options = WatchdogOptions.CreateDefault();
            configureOptions(options);

            return AddWatchdog(services, options);
        }

        /// <summary>
        /// Add watchdog for development environment
        /// </summary>
        public static IServiceCollection AddWatchdogForDevelopment(this IServiceCollection services)
        {
            return AddWatchdog(services, WatchdogOptions.CreateDevelopment());
        }

        /// <summary>
        /// Add watchdog for production environment
        /// </summary>
        public static IServiceCollection AddWatchdogForProduction(this IServiceCollection services)
        {
            return AddWatchdog(services, WatchdogOptions.CreateProduction());
        }
    }
}