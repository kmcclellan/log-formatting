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

    /// <summary>
    /// Configures formatting using the log category, level, and event ID.
    /// </summary>
    /// <param name="action">A delegate to format the entry.</param>
    /// <returns>The same builder, for chaining.</returns>
    public LoggingFormatBuilder<TFormat> OnEntry(Action<TFormat, string, LogLevel, EventId> action)
    {
        this.AddFormatter(new LogFieldFormatter<TFormat>(action));
        return this;
    }

    /// <summary>
    /// Configures formatting using the log message.
    /// </summary>
    /// <param name="action">A delegate to format the entry.</param>
    /// <returns>The same builder, for chaining.</returns>
    public LoggingFormatBuilder<TFormat> OnMessage(Action<TFormat, string> action)
    {
        this.AddFormatter(new LogMessageFormatter<TFormat>(action));
        return this;
    }

    /// <summary>
    /// Configures formatting using a logged exception.
    /// </summary>
    /// <param name="action">A delegate to format the entry.</param>
    /// <param name="recursive">Whether to format inner exceptions.</param>
    /// <returns>The same builder, for chaining.</returns>
    public LoggingFormatBuilder<TFormat> OnException(Action<TFormat, Exception> action, bool recursive = false)
    {
        this.AddFormatter(new LogExceptionFormatter<TFormat>(action, recursive));
        return this;
    }

    /// <summary>
    /// Configures formatting using a specific logged exception.
    /// </summary>
    /// <typeparam name="T">The logged exception type.</typeparam>
    /// <param name="action">A delegate to format the entry.</param>
    /// <param name="recursive">Whether to format inner exceptions.</param>
    /// <returns>The same builder, for chaining.</returns>
    public LoggingFormatBuilder<TFormat> OnException<T>(Action<TFormat, T> action, bool recursive = false)
        where T : Exception
    {
        return this.OnException(
            (entry, exception) =>
            {
                if (exception is T match)
                {
                    action(entry, match);
                }
            },
            recursive);
    }

    /// <summary>
    /// Configures formatting using a named log property (e.g. message template argument).
    /// </summary>
    /// <param name="action">A delegate to format the entry.</param>
    /// <param name="category">
    /// A pattern to filter properties by category. Supports prefix and wildcard ('*') matches.
    /// </param>
    /// <returns>The same builder, for chaining.</returns>
    public LoggingFormatBuilder<TFormat> OnProperty(
        Action<TFormat, KeyValuePair<string, object?>> action,
        string category = "*")
    {
        this.AddFormatter(new LogPropertyFormatter<TFormat>(action, category));
        return this;
    }

    /// <summary>
    /// Configures formatting using a specific named log property (e.g. message template argument).
    /// </summary>
    /// <typeparam name="TCategory">
    /// The type corresponding to the log category of the property (e.g. <see cref="ILogger{TCategoryName}"/>).
    /// </typeparam>
    /// <param name="name">The log property name</param>
    /// <param name="action">A delegate to format the entry.</param>
    /// <returns>The same builder, for chaining.</returns>
    public LoggingFormatBuilder<TFormat> OnProperty<TCategory>(string name, Action<TFormat, object?> action)
    {
        return this.OnProperty(
            (entry, kvp) =>
            {
                if (kvp.Key == name)
                {
                    action(entry, kvp.Value);
                }
            },
            LogCategory.ForType<TCategory>());
    }

    void AddFormatter(ILogFormatter<TFormat> formatter)
    {
        this.Services.Configure<LogFormatOptions<TFormat>>(x => x.Formatters.Add(formatter));
    }
}
