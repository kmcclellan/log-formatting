using Logging.Formatting.Example;
using Logging.Formatting.Example.Farming;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Formatting;

var services = new ServiceCollection();

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

        builder.ClearProviders();

        builder.Serialize(x => x.Serializer = new FarmLogSerializer())
            .ToFile();

        builder.Services.AddSingleton<ILoggerProvider, UploadLoggerProvider>();
    });

// Use service container to resolve formatter dependencies.
services.AddSingleton<WeatherGauge>()
    .AddOptions<LogFormatOptions<FarmLog>>()
    .Configure<WeatherGauge>((x, y) => x.Formatters.Add(new WeatherFormatter(y)));

await using var provider = services.BuildServiceProvider();
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
