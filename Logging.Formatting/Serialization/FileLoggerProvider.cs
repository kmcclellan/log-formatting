namespace Microsoft.Extensions.Logging.Serialization;

/// <summary>
/// A provider of loggers using file serialization.
/// </summary>
/// <typeparam name="TFormat">The formatted entry type.</typeparam>
public sealed class FileLoggerProvider<TFormat> : ILoggerProvider, IAsyncDisposable
{
    static readonly FileStreamOptions StreamOptions =
        new()
        {
            Mode = FileMode.Append,
            Access = FileAccess.Write,
            Share = FileShare.None,
            Options = FileOptions.Asynchronous | FileOptions.WriteThrough,
            BufferSize = 0,
        };

    readonly IBufferLoggerProvider<TFormat> loggers;
    readonly Task completion;

    /// <summary>
    /// Initializes the provider.
    /// </summary>
    /// <param name="loggers">The formatted logger provider.</param>
    /// <param name="serializer">The log serializer.</param>
    /// <param name="options">The file logging options.</param>
    /// <param name="environment">The host environment.</param>
    public FileLoggerProvider(
        IBufferLoggerProvider<TFormat> loggers,
        ILogSerializer<TFormat> serializer,
        IOptions<FileLoggingOptions> options,
        IHostEnvironment? environment = null)
    {
        this.loggers = loggers ?? throw new ArgumentNullException(nameof(loggers));
        this.completion = Flush(
            loggers.Buffer,
            serializer ?? throw new ArgumentNullException(nameof(serializer)),
            options ?? throw new ArgumentNullException(nameof(options)),
            environment);
    }

    // Use same fallback as Host
    // https://github.com/dotnet/runtime/blob/d037e070ebe5c83838443f869d5800752b0fcb13/src/libraries/Microsoft.Extensions.Hosting/src/HostBuilder.cs#L235
    static string FallbackAppName => Assembly.GetEntryAssembly()?.GetName()?.Name ?? "dotnet";

    /// <inheritdoc/>
    public ILogger CreateLogger(string categoryName)
    {
        return this.loggers.CreateLogger(categoryName);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        this.loggers.Buffer.Complete();
        this.completion.Wait();
    }

    /// <inheritdoc/>
    public ValueTask DisposeAsync()
    {
        this.loggers.Buffer.Complete();
        return new(this.completion);
    }

    static async Task Flush(
        IReceivableSourceBlock<TFormat> entries,
        ILogSerializer<TFormat> serializer,
        IOptions<FileLoggingOptions> options,
        IHostEnvironment? environment)
    {
        try
        {
            var formatArgs = environment != null
                ? new object[] { environment.ApplicationName, environment.EnvironmentName }
                : new object[] { FallbackAppName, "Production" };

            while (!entries.Completion.IsCompleted)
            {
                var output = await serializer.SerializeAsync(entries).ConfigureAwait(false);
                var path = Path.Combine(
                    AppContext.BaseDirectory,
                    string.Format(CultureInfo.InvariantCulture, options.Value.Path, formatArgs));

                var retries = 0;

                do
                {
                    try
                    {
                        using var file = File.Open(path, StreamOptions);
                        await file.WriteAsync(output).ConfigureAwait(false);
                    }
                    catch (IOException exception) when (retries++ < 3)
                    {
                        // File might be locked by another process.
                        await Console.Error.WriteLineAsync($"Error flushing logs: {exception}").ConfigureAwait(false);
                        await Task.Delay(100).ConfigureAwait(false);
                        continue;
                    }

                    retries = 0;
                }
                while (retries > 0);
            }
        }
        catch (Exception exception)
        {
            entries.Fault(exception = new IOException("Failed to flush buffered log entries.", exception));
            throw exception;
        }
    }
}
