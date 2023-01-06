namespace Microsoft.Extensions.Logging.Formatting;

sealed class BufferLoggerProvider<TFormat> : IBufferLoggerProvider<TFormat>
{
    static readonly DataflowBlockOptions BufferOptions =
        new()
        {
            EnsureOrdered = false,
        };

    readonly IOptions<LogFormatOptions<TFormat>> options;
    readonly BufferBlock<TFormat> buffer = new(BufferOptions);

    public BufferLoggerProvider(IOptions<LogFormatOptions<TFormat>> options)
    {
        this.options = options;
    }

    public IReceivableSourceBlock<TFormat> Buffer => this.buffer;

    public ILogger CreateLogger(string categoryName)
    {
        var opts = this.options.Value;
        var formatters = opts.Formatters.Where(x => x.IsEnabled(categoryName)).ToArray();

        if (formatters.Length == 0)
        {
            return NullLogger.Instance;
        }

        if (opts.Factory == null)
        {
            throw new InvalidOperationException(
                $"{nameof(LogFormatOptions<TFormat>)}.{nameof(opts.Factory)} is required.");
        }

        var stacks = opts.Formatters
            .Select(x => x.Scopes)
            .OfType<ILogStackFormatter<TFormat>>()
            .ToArray();

        return new FormattedLogger(opts.Factory, this.buffer, categoryName, formatters, stacks);
    }

    public void Dispose()
    {
    }

    sealed class FormattedLogger : ILogger
    {
        readonly Func<TFormat> factory;
        readonly BufferBlock<TFormat> buffer;
        readonly string category;
        readonly ILogFormatter<TFormat>[] formatters;
        readonly ILogStackFormatter<TFormat>[] stacks;

        public FormattedLogger(
            Func<TFormat> factory,
            BufferBlock<TFormat> buffer,
            string category,
            ILogFormatter<TFormat>[] formatters,
            ILogStackFormatter<TFormat>[] stacks)
        {
            this.factory = factory;
            this.category = category;
            this.buffer = buffer;
            this.formatters = formatters;
            this.stacks = stacks;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return this.formatters.Length > 0;
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            var stacks = default(List<ILogStackFormatter<TFormat>>);

            foreach (var formatter in this.formatters)
            {
                if (formatter.Scopes != null && formatter.Scopes.TryPush(this.category, state))
                {
                    (stacks ??= new()).Add(formatter.Scopes);
                }
            }

            return stacks != null ? new StackPopper(stacks) : null;
        }

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            if (this.formatters.Length > 0)
            {
                var entry = this.factory();

                foreach (var stack in this.stacks)
                {
                    stack.Format(entry);
                }

                var raw = new LogEntry<TState>(logLevel, this.category, eventId, state, exception, formatter);

                foreach (var fmtr in this.formatters)
                {
                    fmtr.Format(entry, raw);
                }

                if (!this.buffer.Post(entry))
                {
                    throw new InvalidOperationException(
                        "Entry buffer is in faulted state.",
                        this.buffer.Completion.Exception?.Flatten());
                }
            }
        }

        sealed class StackPopper : IDisposable
        {
            readonly IReadOnlyList<ILogStackFormatter<TFormat>> stacks;

            bool disposed;

            public StackPopper(IReadOnlyList<ILogStackFormatter<TFormat>> stacks)
            {
                this.stacks = stacks;
            }

            public void Dispose()
            {
                if (!disposed)
                {
                    for (var i = 0; i < stacks.Count; i++)
                    {
                        stacks[i].Pop();
                    }

                    disposed = true;
                }
            }
        }
    }
}
