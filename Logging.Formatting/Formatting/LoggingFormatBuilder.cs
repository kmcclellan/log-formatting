namespace Microsoft.Extensions.Logging.Formatting;

/// <summary>
/// A component to configure logging providers using formatted log entries.
/// </summary>
/// <typeparam name="TFormat">The formatted entry type.</typeparam>
public class LoggingFormatBuilder<TFormat> : ILoggingBuilder
{
    internal LoggingFormatBuilder(IServiceCollection services)
    {
        this.Services = services;
    }

    /// <inheritdoc/>
    public IServiceCollection Services { get; }
}
