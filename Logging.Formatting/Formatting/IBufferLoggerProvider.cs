namespace Microsoft.Extensions.Logging.Formatting;

/// <summary>
/// Creates loggers that write formatted entries to a buffer.
/// </summary>
/// <typeparam name="TFormat">The formatted entry type.</typeparam>
public interface IBufferLoggerProvider<TFormat> : ILoggerProvider
{
    /// <summary>
    /// Gets the source dataflow block for buffered entries.
    /// </summary>
    IReceivableSourceBlock<TFormat> Buffer { get; }
}
