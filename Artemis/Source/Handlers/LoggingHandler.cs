
using NetCord.Gateway;
using NetCord.Hosting.Gateway;
using Serilog;
using Serilog.Events;

namespace Handlers;

[GatewayEvent(nameof(GatewayClient.Log))]
public class LoggingHandler : IGatewayEventHandler<LogMessage>
{
    public ValueTask HandleAsync(LogMessage message)
    {
        var severity = message.Severity switch
        {
            LogSeverity.Info => LogEventLevel.Information,
            LogSeverity.Error => LogEventLevel.Error,
            _ => LogEventLevel.Information
        };

        Log.Write(severity, message.Exception, "[NetCord] {Message}", message.Message);
        return default;
    }
}
