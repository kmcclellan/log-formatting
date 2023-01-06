namespace Microsoft.Extensions.Logging.Serialization;

sealed class LogSerializer<TFormat> : ILogSerializer<TFormat>
{
    readonly IOptions<LogSerializeOptions<TFormat>> options;
    readonly ILogEntrySerializer<TFormat> entrySerializer;

    public LogSerializer(IOptions<LogSerializeOptions<TFormat>> options)
    {
        this.options = options;
        this.entrySerializer = options.Value.Serializer ??
            throw new InvalidOperationException(
                $"{nameof(LogFormatOptions<TFormat>)}.{nameof(options.Value.Serializer)} is required.");
    }

    public async Task<ReadOnlyMemory<byte>> SerializeAsync(IReceivableSourceBlock<TFormat> items)
    {
        var output = default(ArrayBufferWriter<byte>);
        var timestamp = DateTime.UtcNow;

        try
        {
            while (TryGetTimeout(output, DateTime.UtcNow - timestamp, out var timeout) &&
                await items.OutputAvailableAsync().WaitAsync(timeout).ConfigureAwait(false))
            {
                output ??= new();
                var delim = this.options.Value.Delimiter;

                while (items.TryReceive(out var entry))
                {
                    this.entrySerializer.Serialize(output, entry);

                    if (delim.Length > 0)
                    {
                        delim.CopyTo(output.GetSpan(delim.Length));
                        output.Advance(delim.Length);
                    }
                }
            }
        }
        catch (TimeoutException)
        {
        }

        return output?.WrittenMemory ?? Memory<byte>.Empty;
    }

    private bool TryGetTimeout(ArrayBufferWriter<byte>? output, TimeSpan elapsed, out TimeSpan timeout)
    {
        if (output == null)
        {
            timeout = Timeout.InfiniteTimeSpan;
            return true;
        }

        var opts = this.options.Value;

        if (output.WrittenCount < opts.BufferSize && elapsed < opts.BufferInterval)
        {
            timeout = opts.BufferInterval - elapsed;
            return true;
        }

        timeout = default;
        return false;
    }
}
