namespace Microsoft.Extensions.Logging.Formatting;

sealed class LogPropertyFormatter<TFormat> : ILogFormatter<TFormat>
{
    readonly Action<TFormat, KeyValuePair<string, object?>> action;
    readonly string? category;

    public LogPropertyFormatter(Action<TFormat, KeyValuePair<string, object?>> action, string? category)
    {
        this.action = action;
        this.category = category;
        this.Scopes = new PropertyStack(this);
    }

    public ILogStackFormatter<TFormat>? Scopes { get; }

    public bool IsEnabled(string category)
    {
        return this.category == null || LogCategory.Matches(this.category, category);
    }

    public void Format<TState>(TFormat entry, in LogEntry<TState> data)
    {
        if (data.State is IReadOnlyList<KeyValuePair<string, object?>> properties)
        {
            this.Format(entry, properties);
        }
    }

    void Format(TFormat entry, IReadOnlyList<KeyValuePair<string, object?>> properties)
    {
        for (var i = 0; i < properties.Count; i++)
        {
            this.action(entry, properties[i]);
        }
    }

    sealed class PropertyStack : ILogStackFormatter<TFormat>
    {
        readonly LogPropertyFormatter<TFormat> formatter;
        readonly AsyncLocal<Stack<IReadOnlyList<KeyValuePair<string, object?>>>> stack = new();

        public PropertyStack(LogPropertyFormatter<TFormat> formatter)
        {
            this.formatter = formatter;
        }

        public bool TryPush<TState>(string category, TState state)
        {
            if (state is IReadOnlyList<KeyValuePair<string, object?>> properties)
            {
                (this.stack.Value ??= new()).Push(properties);
                return true;
            }

            return false;
        }

        public void Pop()
        {
            if (this.stack.Value == null || this.stack.Value.Count == 0)
            {
                throw new InvalidOperationException("Stack is empty.");
            }

            this.stack.Value.Pop();
        }

        public void Format(TFormat entry)
        {
            if (this.stack.Value != null)
            {
                foreach (var properties in this.stack.Value)
                {
                    this.formatter.Format(entry, properties);
                }
            }
        }
    }
}
