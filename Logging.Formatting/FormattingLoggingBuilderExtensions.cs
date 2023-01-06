namespace Microsoft.Extensions.Logging;

/// <summary>
/// Extensions of <see cref="ILoggingBuilder"/> to facilitate log formatting.
/// </summary>
public static class FormattingLoggingBuilderExtensions
{
    /// <summary>
    /// Adds services for log formatting.
    /// </summary>
    /// <typeparam name="TFormat">The formatted entry type.</typeparam>
    /// <param name="builder">The logging builder.</param>
    /// <returns>A builder for formatted logging.</returns>
    public static LoggingFormatBuilder<TFormat> Format<TFormat>(this ILoggingBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder, nameof(builder));
        builder.Services.AddTransient<IBufferLoggerProvider<TFormat>, BufferLoggerProvider<TFormat>>();
        return new(builder.Services);
    }
}
