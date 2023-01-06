namespace Microsoft.Extensions.Logging.Serialization;

/// <summary>
/// Serializes individual formatted log entries.
/// </summary>
/// <typeparam name="TFormat">The formatted entry type.</typeparam>
public interface ILogEntrySerializer<in TFormat>
{
    /// <summary>
    /// Serializes a formatted entry.
    /// </summary>
    /// <param name="writer">The binary writer to use.</param>
    /// <param name="entry">The formatted entry.</param>
    void Serialize(IBufferWriter<byte> writer, TFormat entry);
}
