namespace Microsoft.Extensions.Logging.Formatting;

static class LogCategory
{
    public static string ForType<TCategory>()
    {
        // Workaround since TypeNameHelper is internal.
        using var factory = new FakeFactory();
        factory.CreateLogger<TCategory>();

        return factory.Category ??
            throw new ArgumentOutOfRangeException(nameof(TCategory), "No category name for type.");
    }

    // Use same prefix/wildcard matching as logger factory.
    // https://github.com/dotnet/runtime/blob/2306813eaf2066fe63cb4766572fc68e80a24ef7/src/libraries/Microsoft.Extensions.Logging/src/LoggerRuleSelector.cs#L52-L77
    public static bool Matches(string pattern, string category)
    {
        const char WildcardChar = '*';
        int wildcardIndex = pattern.IndexOf(WildcardChar, StringComparison.Ordinal);

        if (wildcardIndex != -1 && pattern.IndexOf(WildcardChar, wildcardIndex + 1) != -1)
        {
            throw new InvalidOperationException("Only one wildcard character is allowed in category name.");
        }

        ReadOnlySpan<char> prefix, suffix;
        if (wildcardIndex == -1)
        {
            prefix = pattern.AsSpan();
            suffix = default;
        }
        else
        {
            prefix = pattern.AsSpan(0, wildcardIndex);
            suffix = pattern.AsSpan(wildcardIndex + 1);
        }

        return category.AsSpan().StartsWith(prefix, StringComparison.OrdinalIgnoreCase) &&
            category.AsSpan().EndsWith(suffix, StringComparison.OrdinalIgnoreCase);
    }

    sealed class FakeFactory : ILoggerFactory
    {
        public string? Category { get; private set; }

        public void AddProvider(ILoggerProvider provider)
        {
            throw new NotSupportedException();
        }

        public ILogger CreateLogger(string categoryName)
        {
            this.Category = categoryName;
            return NullLogger.Instance;
        }

        public void Dispose()
        {
        }
    }
}
