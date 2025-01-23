
using System.Diagnostics;
using Hangfire;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;
using Serilog;

namespace Handlers;

[GatewayEvent(nameof(GatewayClient.Ready))]
public class ReadyHandler(GatewayClient client, Config config, Stopwatch stopwatch, IRecurringJobManager recurringJob) : IGatewayEventHandler<ReadyEventArgs>
{
    private readonly GatewayClient Client = client;
    private readonly Config Config = config;
    private readonly Stopwatch Stopwatch = stopwatch;
    private readonly IRecurringJobManager RecurringJob = recurringJob;

    public async ValueTask HandleAsync(ReadyEventArgs ready)
    {
        // Start the uptime stopwatch.
        Stopwatch.Start();

        Log.Information("Connected to Discord as {Username}#{Discriminator}", ready.User.Username, ready.User.Discriminator);
        
        // Set a random presence.
        await SetRandomPresenceAsync();

        // Hangfire Jobs.
        RecurringJob.AddOrUpdate("SetRandomPresence", () => SetRandomPresenceAsync(), Cron.Minutely);
        Database.Database.CreateBackup();
        RecurringJob.AddOrUpdate("Database Backup", () => Database.Database.CreateBackup(), Cron.Daily);
    }

    public async Task SetRandomPresenceAsync()
    {
        var presence = Config.ClientPresences.OrderBy(x => Guid.NewGuid()).ToArray().First();

        await Client.UpdatePresenceAsync(
            new PresenceProperties(presence.Status).WithActivities(
                [
                    new(presence.Text, presence.Type)
                ]
            )
        );
    }
}
