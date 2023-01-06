namespace Microsoft.Extensions.Logging.Serialization;

sealed class JsonLogSerializer<TFormat> : ILogEntrySerializer<TFormat>
{
    readonly JsonSerializerOptions options;

    public JsonLogSerializer(JsonSerializerOptions options)
    {
        this.options = options;
    }

    public void Serialize(IBufferWriter<byte> writer, TFormat entry)
    {
        using var json = new Utf8JsonWriter(writer, GetWriterOptions(this.options));
        JsonSerializer.Serialize(json, entry, this.options);
    }

    // Use same writer options as JsonSerializer.
    // https://github.com/dotnet/runtime/blob/2306813eaf2066fe63cb4766572fc68e80a24ef7/src/libraries/System.Text.Json/src/System/Text/Json/Serialization/JsonSerializerOptions.cs#L650-L658
    static JsonWriterOptions GetWriterOptions(JsonSerializerOptions options)
    {
        return new JsonWriterOptions
        {
            Encoder = options.Encoder,
            Indented = options.WriteIndented,
#if NET7_0_OR_GREATER
            MaxDepth = options.MaxDepth == 0 ? 64 : options.MaxDepth,
#endif
#if !DEBUG
                SkipValidation = true
#endif
        };
    }
}
