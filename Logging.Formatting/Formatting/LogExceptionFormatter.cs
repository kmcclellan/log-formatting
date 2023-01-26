namespace Microsoft.Extensions.Logging.Formatting;

sealed class LogExceptionFormatter<TFormat> : ILogFormatter<TFormat>
{
    readonly Action<TFormat, Exception> action;
    readonly bool recursive;

    public LogExceptionFormatter(Action<TFormat, Exception> action, bool recursive)
    {
        this.action = action;
        this.recursive = recursive;
    }

    public ILogStackFormatter<TFormat>? Scopes => null;

    public bool IsEnabled(string category)
    {
        return true;
    }

    public void Format<TState>(TFormat entry, in LogEntry<TState> data)
    {
        if (data.Exception != null)
        {
            this.Map(entry, data.Exception);
        }
    }

    void Map(TFormat entry, Exception exception)
    {
        if (this.recursive && exception is AggregateException aggregate)
        {
            foreach (var inner in aggregate.InnerExceptions)
            {
                this.Map(entry, inner);
            }
        }
        else
        {
            this.action(entry, exception);

            if (this.recursive && exception.InnerException != null)
            {
                this.Map(entry, exception.InnerException);
            }
        }
    }
}
