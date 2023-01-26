# .NET Log Formatting
An extension of `Microsoft.Extensions.Logging` to format log information into typed entries.

### Features
* Select and transform log data into entries using a fluent API.
* Implement logger providers handling formatted entries.
* Convert batches of entries to binary data using JSON or a custom serializer.
* Direct serialized output to file system or a custom destination.

## Installation

Add the NuGet package to your project:

    $ dotnet add package Logging.Formatting

## Usage

Though .NET supports implementing custom logger providers, it's fairly limited in what tools it provides to work with log information.

In particular, it's often desireable to define the _format_ of log entries independently of their target/destination. Using generics, log formatting enables you to represent individual log entries using any mutable reference type - even one contained in a third party library.

### Configuring the log format

Consider this (bucolic, somewhat overwrought) example:

```c#
var logger = provider.GetRequiredService<ILogger<Program>>();

foreach (var animal in new[] { "cows", "pigs", "chickens" })
{
    using (logger.BeginScope("Tending the {Animal}", animal))
    {
        if (animal == "cows")
        {
            logger.LogInformation(FarmLog.Milking, "{MilkBuckets} buckets of milk", 3);
        }

        logger.LogInformation(FarmLog.Feeding, "Fat and happy", animal);
    }
}

logger.LogWarning(FarmLog.CheckSupply, "Running low on {Supply}", "chicken feed");
logger.LogError(new TractorIsBrokenException("carburetor"), "Could not start tractor");
```

Enable formatted logging using the typed overload of [`IServiceCollection.AddLogging(...)`](/Logging.Formatting/FormattingServiceCollectionExtensions.cs). Specify the format using the methods of [`LoggingFormatBuilder<TFormat>`](/Logging.Formatting/Formatting/LoggingFormatBuilder.cs). 

```c#
services.AddLogging(
    () => new FarmLog(),
    builder =>
    {
        builder.OnMessage((x, y) => x.Notes.Add(y));

        builder.OnEntry(
            (log, cat, lvl, id) =>
            {
                if (id == FarmLog.CheckSupply)
                {
                    log.Location = "Barn";

                    if (lvl == LogLevel.Warning)
                    {
                        log.Notes.Add("Need to go to the store");
                    }
                }
            });

        // Properties are category scoped to prevent name collisions.
        builder.OnProperty<Program>(
            "Animal",
            (log, obj) =>
            {
                log.Animal = (string)obj;

                switch (obj)
                {
                    case "cows":
                        log.Location = "Field";
                        break;

                    case "pigs":
                    case "chickens":
                        log.Location = "Barn";
                        break;
                }
            });

        builder.OnException<TractorIsBrokenException>(
            (log, ex) =>
            {
                log.Location = "Garage";
                log.Notes.Add($"Need to fix the {ex.PartName}");
            });

        // Configure logging providers...
    });
```

For even greater control over log format, implement one or more custom log formatters.

```c#
class WeatherFormatter : ILogFormatter<FarmLog>
{
    readonly WeatherGauge gauge;

    public WeatherFormatter(WeatherGauge gauge)
    {
        this.gauge = gauge;
    }

    // Scope formatting is optional.
    // Implementing a stack formatter involves both capturing state and enriching logs.
    public ILogStackFormatter<FarmLog> Scopes => null;

    // Filter by category if you want to target only certain loggers (avoids some formatting overhead).
    public bool IsEnabled(string category)
    {
        return true;
    }

    // Use state and/or the raw entry data to modify the typed entry.
    public void Format<TState>(FarmLog entry, in LogEntry<TState> data)
    {
        var rainy = this.gauge.IsRaining ? "rainy" : null;
        var heat = gauge.Temperature switch
        {
            > 60 => "hot",
            < 40 => "cold",
            _ => null,
        };

        entry.Weather = heat != null && rainy != null ? $"{heat} and {rainy}" : heat ?? rainy;
    }
}
```

The logging format builder provides a fluent API for the underlying [`LogFormatOptions<TFormat>`](/Logging.Formatting/Formatting/LogFormatOptions.cs), which can be also be configured directly.

```c#
// Use service container to resolve formatter dependencies.
services.AddSingleton<WeatherGauge>()
    .AddOptions<LogFormatOptions<FarmLog>>()
    .Configure<WeatherGauge>((x, y) => x.Formatters.Add(new WeatherFormatter(y)));
```

### Writing formatted entries to a file

You can configure this library to persist formatted entries to the file system. [`LogSerializeOptions<TFormat>`](/Logging.Formatting/Serialization/LogSerializeOptions.cs) and [`FileLoggingOptions`](/Logging.Formatting/Serialization/FileLoggingOptions.cs) provide some control over this behavior.

```c#
builder.Serialize(x => x.AsJson()).ToFile();
```

This generates output from the above example as follows:

```json
{"HourOfDay":12,"Location":"Field","Animal":"cows","Weather":"cold and rainy","Notes":["3 buckets of milk"]}
{"HourOfDay":12,"Location":"Field","Animal":"cows","Weather":"cold and rainy","Notes":["Fat and happy"]}
{"HourOfDay":12,"Location":"Barn","Animal":"pigs","Weather":"cold and rainy","Notes":["Fat and happy"]}
{"HourOfDay":12,"Location":"Barn","Animal":"chickens","Weather":"cold and rainy","Notes":["Fat and happy"]}
{"HourOfDay":12,"Location":"Barn","Animal":null,"Weather":"cold and rainy","Notes":["Running low on chicken feed","Need to go to the store"]}
{"HourOfDay":12,"Location":"Garage","Animal":null,"Weather":"cold and rainy","Notes":["Could not start tractor","Need to fix the carburetor"]}
```

If JSON doesn't suit your needs, implement and configure a custom entry serializer:

```c#
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
```

```c#
builder.Serialize(x => x.Serializer = new FarmLogSerializer())
    .ToFile();
```

```
| 12 |     cows |              3 buckets of milk |
| 12 |     cows |                  Fat and happy |
| 12 |     pigs |                  Fat and happy |
| 12 | chickens |                  Fat and happy |
| 12 |          |    Running low on chicken feed |
| 12 |          |        Could not start tractor |
```

### Writing formatted entries elsewhere

Though file logging is currently the only use case with a built-in provider, you can write a custom logger provider using one or both of [`IBufferLoggerProvider<TFormat>`](/Logging.Formatting/Formatting/IBufferLoggerProvider.cs) and [`ILogSerializer<TFormat>`](/Logging.Formatting/Serialization/ILogSerializer.cs) to output formatted entries to different destinations.

An example provider uploading log data using HTTP requests:

```c#
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
```
