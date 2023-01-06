namespace Microsoft.Extensions.Logging.Formatting;

/// <summary>
/// Options for formatting log information into typed entries.
/// </summary>
/// <typeparam name="TFormat">The formatted entry type.</typeparam>
public class LogFormatOptions<TFormat>
{
    /// <summary>
    /// Gets or sets the delegate for initializing log entries.
    /// </summary>
    public Func<TFormat>? Factory { get; set; }

    /// <summary>
    /// Gets the log formatter collection.
    /// </summary>
    public IList<ILogFormatter<TFormat>> Formatters { get; } = new List<ILogFormatter<TFormat>>();
}
