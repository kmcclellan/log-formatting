using Logging.Formatting.Example.Farming;

using Microsoft.Extensions.Logging.Serialization;

using System.Buffers;
using System.Text;

class FarmLogSerializer : ILogEntrySerializer<FarmLog>
{
    readonly Encoding encoding = Encoding.UTF8;

    public void Serialize(IBufferWriter<byte> writer, FarmLog entry)
    {
        var payload = $"| {entry.HourOfDay,2} | {entry.Animal,8} | {entry.Notes[0],30} |";

        writer.Advance(
            this.encoding.GetBytes(
                payload,
                writer.GetSpan(this.encoding.GetMaxByteCount(payload.Length))));
    }
}
