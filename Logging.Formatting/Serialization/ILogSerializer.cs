namespace Microsoft.Extensions.Logging.Serialization;

/// <summary>
/// Serializes formatted log entries into binary data.
/// </summary>
/// <typeparam name="TFormat">The formatted entry type.</typeparam>
public interface ILogSerializer<TFormat>
{
    /// <summary>
    /// Serializes a batch of entries into memory.
    /// </summary>
    /// <param name="entries">The source block from which to receive entries.</param>
    /// <returns>A task representing the asynchronous serialization, with resulting memory buffer.</returns>
    Task<ReadOnlyMemory<byte>> SerializeAsync(IReceivableSourceBlock<TFormat> entries);
}
