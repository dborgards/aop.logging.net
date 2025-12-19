using AOP.Logging.Core.Configuration;
using AOP.Logging.Core.Interfaces;
using AOP.Logging.Core.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AOP.Logging.DependencyInjection;

/// <summary>
/// Extension methods for setting up AOP logging services in an <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds AOP logging services to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddAopLogging(this IServiceCollection services)
    {
        return services.AddAopLogging(_ => { });
    }

    /// <summary>
    /// Adds AOP logging services to the specified <see cref="IServiceCollection"/> with configuration.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="configure">An action to configure the <see cref="AopLoggingOptions"/>.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddAopLogging(
        this IServiceCollection services,
        Action<AopLoggingOptions> configure)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        if (configure == null)
        {
            throw new ArgumentNullException(nameof(configure));
        }

        // Register the options
        services.Configure(configure);

        // Register the method logger
        services.TryAddSingleton<IMethodLogger, DefaultMethodLogger>();

        // Add post-configuration to inject method logger into services
        services.AddOptions<AopLoggingServiceConfiguration>()
            .PostConfigure<IServiceProvider>((options, serviceProvider) =>
            {
                options.ServiceProvider = serviceProvider;
            });

        return services;
    }

    /// <summary>
    /// Adds AOP logging to a service with automatic method logger injection.
    /// </summary>
    /// <typeparam name="TService">The type of the service to add.</typeparam>
    /// <typeparam name="TImplementation">The type of the implementation to use.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the service to.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddTransientWithLogging<TService, TImplementation>(
        this IServiceCollection services)
        where TService : class
        where TImplementation : class, TService
    {
        services.AddTransient<TService>(sp =>
        {
            var implementation = ActivatorUtilities.CreateInstance<TImplementation>(sp);
            InjectMethodLogger(implementation, sp);
            return implementation;
        });
        return services;
    }

    /// <summary>
    /// Adds AOP logging to a scoped service with automatic method logger injection.
    /// </summary>
    /// <typeparam name="TService">The type of the service to add.</typeparam>
    /// <typeparam name="TImplementation">The type of the implementation to use.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the service to.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddScopedWithLogging<TService, TImplementation>(
        this IServiceCollection services)
        where TService : class
        where TImplementation : class, TService
    {
        services.AddScoped<TService>(sp =>
        {
            var implementation = ActivatorUtilities.CreateInstance<TImplementation>(sp);
            InjectMethodLogger(implementation, sp);
            return implementation;
        });
        return services;
    }

    /// <summary>
    /// Adds AOP logging to a singleton service with automatic method logger injection.
    /// </summary>
    /// <typeparam name="TService">The type of the service to add.</typeparam>
    /// <typeparam name="TImplementation">The type of the implementation to use.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the service to.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddSingletonWithLogging<TService, TImplementation>(
        this IServiceCollection services)
        where TService : class
        where TImplementation : class, TService
    {
        services.AddSingleton<TService>(sp =>
        {
            var implementation = ActivatorUtilities.CreateInstance<TImplementation>(sp);
            InjectMethodLogger(implementation, sp);
            return implementation;
        });
        return services;
    }

    private static void InjectMethodLogger(object implementation, IServiceProvider serviceProvider)
    {
        var methodLogger = serviceProvider.GetService<IMethodLogger>();
        if (methodLogger == null)
        {
            return;
        }

        // Try to find and invoke SetMethodLogger method
        var setMethodLoggerMethod = implementation.GetType()
            .GetMethod("SetMethodLogger", new[] { typeof(IMethodLogger) });

        setMethodLoggerMethod?.Invoke(implementation, new object[] { methodLogger });
    }
}

/// <summary>
/// Internal configuration for AOP logging services.
/// </summary>
internal class AopLoggingServiceConfiguration
{
    public IServiceProvider? ServiceProvider { get; set; }
}
