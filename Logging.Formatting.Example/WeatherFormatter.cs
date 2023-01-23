namespace Logging.Formatting.Example;

using Logging.Formatting.Example.Farming;

using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Formatting;

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
