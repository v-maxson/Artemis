using NetCord.Gateway;
using NetCord.Hosting.Gateway;
using NetCord;
using Artemis.DB.Models;
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
                    EmbedHelper.Embed(
                        title: "User Joined",
                        fields: [
                            EmbedHelper.Field("User:", user.ToString()),
                            EmbedHelper.Field("ID:", user.Id.ToString()),
                            EmbedHelper.Field("Created At:", $"<t:{user.CreatedAt.ToUnixTimeSeconds()}>")
                        ],
                        thumbnail: thumbnail == null ? null : EmbedHelper.Thumbnail(thumbnail.ToString()),
                        timestamp: DateTimeOffset.UtcNow,
                        color: Colors.Green
                    )
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
                    EmbedHelper.Embed(
                        title: "User Left",
                        fields: [
                            EmbedHelper.Field("User:", $"{userLeft.User} ({userLeft.User.Username}{(userLeft.User.Discriminator == 0 ? "" : $"#{userLeft.User.Discriminator}")})"),
                            EmbedHelper.Field("ID:", userLeft.User.Id.ToString()),
                            EmbedHelper.Field("Created At:", $"<t:{userLeft.User.CreatedAt.ToUnixTimeSeconds()}>")
                        ],
                        thumbnail: thumbnail == null ? null : EmbedHelper.Thumbnail(thumbnail.ToString()),
                        timestamp: DateTimeOffset.UtcNow,
                        color: Colors.Red
                    )
                ]
            )
        );
    }
}
