using Logging.Formatting.Example.Farming;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Formatting;
using Microsoft.Extensions.Logging.Serialization;

using System.Threading.Tasks.Dataflow;

class UploadLoggerProvider : ILoggerProvider
{
    readonly IBufferLoggerProvider<FarmLog> loggers;
    readonly Task completion;

    public UploadLoggerProvider(IBufferLoggerProvider<FarmLog> loggers, ILogSerializer<FarmLog> serializer)
    {
        this.loggers = loggers;
        this.completion = Upload(loggers.Buffer, serializer);
    }

    public ILogger CreateLogger(string categoryName)
    {
        return this.loggers.CreateLogger(categoryName);
    }

    public void Dispose()
    {
        this.loggers.Buffer.Complete();
        this.completion.Wait();
    }

    static async Task Upload(IReceivableSourceBlock<FarmLog> entries, ILogSerializer<FarmLog> serializer)
    {
        using var client = new HttpClient();

        try
        {
            while (!entries.Completion.IsCompleted)
            {
                var output = await serializer.SerializeAsync(entries);

                // Would use a different URL for actual logging.
                using var response = await client.PostAsync(
                    "http://httpstat.us/200",
                    new ByteArrayContent(output.ToArray()));

                response.EnsureSuccessStatusCode();
            }
        }
        catch (Exception exception)
        {
            // Propagating the exception to the buffer causes the next log statement to throw.
            entries.Fault(exception = new IOException("Failed to upload buffered log entries.", exception));

            // Throw too so that flush failures bubble up on dispose.
            throw exception;
        }
    }
}
