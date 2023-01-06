namespace Microsoft.Extensions.Logging.Formatting;

/// <summary>
/// Formats log information into typed entries.
/// </summary>
/// <typeparam name="TFormat">The formatted entry type.</typeparam>
public interface ILogFormatter<in TFormat>
{
    /// <summary>
    /// Gets the formatter for log scopes, if any.
    /// </summary>
    ILogStackFormatter<TFormat>? Scopes { get; }

    /// <summary>
    /// Checks if this formatter is enabled.
    /// </summary>
    /// <param name="category">The log category name.</param>
    /// <returns><see langword="true"/> if enabled, otherwise <see langword="false"/>.</returns>
    bool IsEnabled(string category);

    /// <summary>
    /// Formats a typed entry using log entry data.
    /// </summary>
    /// <typeparam name="TState">The log state type.</typeparam>
    /// <param name="entry">The formatted entry.</param>
    /// <param name="data">The log entry data.</param>
    void Format<TState>(TFormat entry, in LogEntry<TState> data);
}
