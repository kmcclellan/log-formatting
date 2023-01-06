namespace Microsoft.Extensions.Logging;

sealed class NullConfigureOptions<T> : IConfigureOptions<T>
    where T : class
{
    public static NullConfigureOptions<T> Instance = new();

    private NullConfigureOptions()
    {
    }

    public void Configure(T options)
    {
    }
}
