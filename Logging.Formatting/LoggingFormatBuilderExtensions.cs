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
}
