using DB.Models;
using NetCord;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;
using NetCord.Rest;

namespace Handlers;

[GatewayEvent(nameof(GatewayClient.MessageDelete))]
public class MessageDeleteHandler(GatewayClient client, Cache.MessageCache cache) : IGatewayEventHandler<MessageDeleteEventArgs>
{
    private readonly GatewayClient Client = client;
    private readonly Cache.MessageCache MessageCache = cache;

    public async ValueTask HandleAsync(MessageDeleteEventArgs messageDelete) {
        if (messageDelete == null) return;

        // Get the guild settings.
        if (!GuildSettings.TryGet((ulong)messageDelete.GuildId!, out var settings)) return;
        if (settings.LogsChannelId == null || !settings.MessageDeleteLogsEnabled) return;

        // Check if the message is cached.
        if (!MessageCache.TryGetValue(messageDelete.MessageId, out Message message)) return;
        if (message is null) return;

        // Get the guild.
        var guild = await Client.GetOrFetchGuildAsync((ulong)messageDelete.GuildId!);
        if (guild is null) return;

        // Get the logs channel.
        var logsChannel = guild.Channels[settings.LogsChannelId.Value];
        if (logsChannel is null) return;

        // Send the log message.
        await ((TextGuildChannel)logsChannel).SendMessageAsync(
            new MessageProperties().WithEmbeds(
                [
                    new EmbedProperties()
                        .WithColor(Colors.Red)
                        .WithTitle("Message Deleted")
                        .WithDescription(message?.Content ?? "Unknown.")
                        .WithFields([
                            new EmbedFieldProperties().WithName("Author:").WithValue(message?.Author.ToString() ?? "Unknown.").WithInline(true),
                            new EmbedFieldProperties().WithName("Channel:").WithValue(message?.Channel?.ToString() ?? "Unknown.").WithInline(true),
                        ])
                        .WithThumbnail(new EmbedThumbnailProperties(message?.Author.GetAvatarUrl()?.ToString()) ?? null)
                        .WithTimestamp(DateTimeOffset.UtcNow)
                ]
            )
        );

        // Remove the message from the cache.
        MessageCache.Remove(messageDelete.MessageId);
    }
}
