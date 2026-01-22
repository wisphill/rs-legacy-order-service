using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

namespace LegacyOrderService.Common;

public static class LegacyOrderServiceLogger
{
    public static IServiceCollection AddLogger(
        this IServiceCollection serviceCollections
    )
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3} {SourceContext}] {Message:lj}{NewLine}{Exception}",
                theme: AnsiConsoleTheme.Sixteen
            )
            .WriteTo.File(
                "logs/app-.log",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7
            )
            .CreateLogger();
            
        serviceCollections.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddSerilog(dispose: true);
        });
        return serviceCollections;
    }
}