namespace Microsoft.Extensions.Logging;

/// <summary>
/// Extensions of <see cref="IServiceCollection"/> to facilitate log formatting.
/// </summary>
public static class FormattingServiceCollectionExtensions
{
    /// <summary>
    /// Adds services for logging using formatted log entries.
    /// </summary>
    /// <typeparam name="TFormat">The formatted entry type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="factory">A delegate for initializing log entries.</param>
    /// <param name="configure">A delegate to configure formatted logging.</param>
    /// <returns>The same services, for chaining.</returns>
    public static IServiceCollection AddLogging<TFormat>(
        this IServiceCollection services,
        Func<TFormat> factory,
        Action<LoggingFormatBuilder<TFormat>> configure)
    {
        ArgumentNullException.ThrowIfNull(services, nameof(services));
        ArgumentNullException.ThrowIfNull(factory, nameof(factory));
        ArgumentNullException.ThrowIfNull(configure, nameof(configure));

        services.Configure<LogFormatOptions<TFormat>>(x => x.Factory = factory);
        services.AddLogging(x => configure(x.Format<TFormat>()));

        return services;
    }
}
