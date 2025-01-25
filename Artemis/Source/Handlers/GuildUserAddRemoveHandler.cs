using NetCord.Gateway;
using NetCord.Hosting.Gateway;
using NetCord;
using DB.Models;
using NetCord.Rest;

namespace Handlers;

[GatewayEvent(nameof(GatewayClient.GuildUserAdd))]
public class GuildUserAddHandler(GatewayClient client) : IGatewayEventHandler<GuildUser>
{
    private readonly GatewayClient Client = client;

    public async ValueTask HandleAsync(GuildUser user) {
        // Get the guild settings.
        if (!GuildSettings.TryGet(user.GuildId, out var settings)) return;

        await Task.WhenAll(
            // Send logs.
            SendLogAsync(user, settings),

            // Assign Auto Role.
            AssignAutoRole(user, settings)
            );
    }

    private async Task AssignAutoRole(GuildUser user, GuildSettings settings) {
        if (settings.AutoRoleId == null) return;

        // Get the guild.
        var guild = await Client.GetOrFetchGuildAsync(user.GuildId);
        if (guild is null) return;

        // Get the role.
        var role = guild.Roles[settings.AutoRoleId.Value];
        if (role is null) return;

        // Assign the role.
        await user.AddRoleAsync(role.Id);
    }

    private async Task SendLogAsync(GuildUser user, GuildSettings settings) {
        if (settings.LogsChannelId == null || !settings.JoinLeaveLogsEnabled) return;

        // Get the guild.
        var guild = await Client.GetOrFetchGuildAsync(user.GuildId);
        if (guild is null) return;

        // Get the logs channel.
        var logsChannel = guild.Channels[settings.LogsChannelId.Value];
        if (logsChannel is null) return;

        // Send the message.
        var thumbnail = user.GetAvatarUrl();
        await ((TextGuildChannel)logsChannel).SendMessageAsync(
            new MessageProperties().WithEmbeds(
                [
                    new EmbedProperties()
                        .WithColor(Colors.Green)
                        .WithTitle("User Joined")
                        .WithFields([
                            new EmbedFieldProperties().WithName("User:").WithValue(user.ToString()),
                            new EmbedFieldProperties().WithName("ID:").WithValue(user.Id.ToString()),
                            new EmbedFieldProperties().WithName("Created At:").WithValue($"<t:{user.CreatedAt.ToUnixTimeSeconds()}>")
                        ])
                        .WithThumbnail(thumbnail == null ? null : new EmbedThumbnailProperties(thumbnail.ToString()))
                        .WithTimestamp(DateTimeOffset.UtcNow)
                ]
            )
        );
    }
}

[GatewayEvent(nameof(GatewayClient.GuildUserRemove))]
public class GuildUserRemoveHandler(GatewayClient client) : IGatewayEventHandler<GuildUserRemoveEventArgs>
{
    private readonly GatewayClient Client = client;

    public async ValueTask HandleAsync(GuildUserRemoveEventArgs userLeft) {
        // Get the guild settings.
        if (!GuildSettings.TryGet(userLeft.GuildId, out var settings)) return;

        // Send Logs.
        await SendLogAsync(userLeft, settings);
    }

    private async Task SendLogAsync(GuildUserRemoveEventArgs userLeft, GuildSettings settings) {
        if (settings.LogsChannelId == null || !settings.JoinLeaveLogsEnabled) return;

        // Get the guild.
        var guild = await Client.GetOrFetchGuildAsync(userLeft.GuildId);
        if (guild is null) return;

        // Get the logs channel.
        var logsChannel = guild.Channels[settings.LogsChannelId.Value];
        if (logsChannel is null) return;

        // Send the message.
        var thumbnail = userLeft.User.GetAvatarUrl();
        await ((TextGuildChannel)logsChannel).SendMessageAsync(
            new MessageProperties().WithEmbeds(
                [
                    new EmbedProperties()
                        .WithColor(Colors.Red)
                        .WithTitle("User Left")
                        .WithFields([
                            new EmbedFieldProperties().WithName("User:").WithValue($"{userLeft.User} ({userLeft.User.Username}{(userLeft.User.Discriminator == 0 ? "" : $"#{userLeft.User.Discriminator}")})"),
                            new EmbedFieldProperties().WithName("ID:").WithValue(userLeft.User.Id.ToString()),
                            new EmbedFieldProperties().WithName("Created At:").WithValue($"<t:{userLeft.User.CreatedAt.ToUnixTimeSeconds()}>")
                        ])
                        .WithThumbnail(thumbnail == null ? null : new EmbedThumbnailProperties(thumbnail.ToString()))
                        .WithTimestamp(DateTimeOffset.UtcNow)
                ]
            )
        );
    }
}
