namespace Microsoft.Extensions.Logging.Formatting;

sealed class LogMessageFormatter<TFormat> : ILogFormatter<TFormat>
{
    readonly Action<TFormat, string> action;

    public LogMessageFormatter(Action<TFormat, string> action)
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
        this.action(entry, data.Formatter(data.State, data.Exception));
    }
}
