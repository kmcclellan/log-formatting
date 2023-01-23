namespace Logging.Formatting.Example.Farming;

class WeatherGauge
{
    public int Temperature => 80 - 10 * Math.Abs(DateTime.Now.Month - 6) + 2 * this.Jitter;

    public bool IsRaining => this.Jitter == 0;

    int Jitter => DateTime.Now.Hour % 3;
}
