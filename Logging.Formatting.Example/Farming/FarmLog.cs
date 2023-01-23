namespace Logging.Formatting.Example.Farming;

using Microsoft.Extensions.Logging;

class FarmLog
{
    public static EventId Milking = new(10, nameof(Milking)),
        Feeding = new(15, nameof(Feeding)),
        CheckSupply = new(30, nameof(CheckSupply));

    public int HourOfDay { get; } = DateTime.Now.Hour;

    public string Location { get; set; }

    public string Animal { get; set; }

    public string Weather { get; set; }

    public List<string> Notes { get; } = new();
}
