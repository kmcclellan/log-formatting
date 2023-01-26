namespace Microsoft.Extensions.Logging.Formatting;

sealed class LogFieldFormatter<TFormat> : ILogFormatter<TFormat>
{
    readonly Action<TFormat, string, LogLevel, EventId> action;

    public LogFieldFormatter(Action<TFormat, string, LogLevel, EventId> action)
    {
        this.action = action;
    }

    public ILogStackFormatter<TFormat>? Scopes => null;

    public bool IsEnabled(string category)
    {
        return true;
    }

    public void Format<TState>(TFormat entry, in LogEntry<TState> data)
    {
        this.action(entry, data.Category, data.LogLevel, data.EventId);
    }
}
