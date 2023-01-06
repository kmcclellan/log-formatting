namespace Microsoft.Extensions.Logging;

/// <summary>
/// Extensions of <see cref="LoggingFormatBuilder{TFormat}"/>.
/// </summary>
public static class LoggingFormatBuilderExtensions
{
    /// <summary>
    /// Adds services to serialize formatted log entries.
    /// </summary>
    /// <typeparam name="TFormat">The formatted entry type.</typeparam>
    /// <param name="builder">The logging builder.</param>
    /// <param name="configure">A delegate to configure log serialization.</param>
    /// <returns>The same builder, for chaining.</returns>
    public static LoggingFormatBuilder<TFormat> Serialize<TFormat>(
        this LoggingFormatBuilder<TFormat> builder,
        Action<LogSerializeOptions<TFormat>>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(builder, nameof(builder));

        builder.Services.AddOptions();

        if (configure != null)
        {
            builder.Services.Configure(configure);
        }

        builder.Services.TryAddSingleton<ILogSerializer<TFormat>, LogSerializer<TFormat>>();
        return builder;
    }

    /// <summary>
    /// Adds the <see cref="FileLoggerProvider{TFormat}"/> to the services.
    /// </summary>
    /// <typeparam name="TFormat">The formatted entry type.</typeparam>
    /// <param name="builder">The logging builder.</param>
    /// <param name="configure">A delegate to configure file logging.</param>
    /// <returns>The same builder, for chaining.</returns>
    public static LoggingFormatBuilder<TFormat> ToFile<TFormat>(
        this LoggingFormatBuilder<TFormat> builder,
        Action<FileLoggingOptions>? configure = null)
    {
        Serialize(builder);

        AddProviderOptions<LogSerializeOptions<TFormat>, FileLoggerProvider<TFormat>>(builder);
        AddProviderOptions<FileLoggingOptions, FileLoggerProvider<TFormat>>(builder);

        builder.Services.AddSingleton<ILoggerProvider, FileLoggerProvider<TFormat>>();
        return builder;
    }

    static void AddProviderOptions<TOptions, TProvider>(ILoggingBuilder builder)
        where TOptions : class
    {
        builder.Services.AddSingleton<IConfigureOptions<TOptions>>(
            provider =>
            {
                // May be null if no logging configuration.
                var config = provider.GetService<ILoggerProviderConfiguration<TProvider>>();

                return config != null
                    ? new ConfigureFromConfigurationOptions<TOptions>(config.Configuration)
                    : NullConfigureOptions<TOptions>.Instance;
            });
    }
}
