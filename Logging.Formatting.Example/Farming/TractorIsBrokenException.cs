namespace Logging.Formatting.Example.Farming;

class TractorIsBrokenException : Exception
{
    public TractorIsBrokenException(string partName)
        : base($"Tractor {partName} is broken.")
    {
        this.PartName = partName;
    }

    public string PartName { get; }
}
