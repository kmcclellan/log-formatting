using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

await using var provider = new ServiceCollection()
    .AddLogging(x => x.ClearProviders())
    .BuildServiceProvider();

var logger = provider.GetRequiredService<ILogger<Program>>();

logger.LogInformation("Hello, world!");
