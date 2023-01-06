namespace Microsoft.Extensions.Logging.Serialization;

/// <summary>
/// Options for serializing log entries.
/// </summary>
public class LogSerializeOptions<TFormat>
{
    /// <summary>
    /// Gets or sets the maximum number of log entry bytes to store in memory.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>4096</c>. The actual size may be less based on <see cref="BufferInterval"/>.
    /// </remarks>
    public int BufferSize { get; set; } = 4096;

    /// <summary>
    /// Gets or sets the maximum log time interval to store in memory.
    /// </summary>
    /// <remarks>
    /// Defaults to one second. The actual interval may be less based on <see cref="BufferSize"/>.
    /// </remarks>
    public TimeSpan BufferInterval { get; set; } = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Gets or sets the bytes used to delimit log entries.
    /// </summary>
    /// <remarks>
    /// Defaults to <see cref="Environment.NewLine"/>.
    /// </remarks>
    public byte[] Delimiter { get; set; } = Encoding.ASCII.GetBytes(Environment.NewLine);

    /// <summary>
    /// Gets or sets the serializer for log entries.
    /// </summary>
    public ILogEntrySerializer<TFormat>? Serializer { get; set; }
}
