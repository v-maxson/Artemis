using Artemis.DB.Models;
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
                    EmbedHelper.Embed(
                        title: "Message Deleted",
                        description: message?.Content ?? "Unknown.",
                        color: Colors.Red,
                        fields: [
                            EmbedHelper.Field("Author:", message?.Author.ToString() ?? "Unknown.", true),
                            EmbedHelper.Field("Channel:", message?.Channel?.ToString() ?? "Unknown.", true)
                        ],
                        thumbnail: new EmbedThumbnailProperties(message?.Author.GetAvatarUrl()?.ToString()) ?? null,
                        timestamp: DateTimeOffset.UtcNow
                    )
                ]
            )
        );

        // Remove the message from the cache.
        MessageCache.Remove(messageDelete.MessageId);
    }
}
