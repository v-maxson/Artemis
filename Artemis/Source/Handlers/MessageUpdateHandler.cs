using Artemis.DB.Models;
using NetCord;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;
using NetCord.Rest;

namespace Handlers;

[GatewayEvent(nameof(GatewayClient.MessageUpdate))]
public class MessageUpdateHandler(GatewayClient client, Cache.MessageCache cache) : IGatewayEventHandler<Message>
{
    private readonly GatewayClient Client = client;
    private readonly Cache.MessageCache MessageCache = cache;

    public async ValueTask HandleAsync(Message currentMessage) {
        if (currentMessage == null || currentMessage.Author.IsBot) return;

        // Get the guild settings.
        if (!GuildSettings.TryGet((ulong)currentMessage.GuildId!, out var settings)) return;
        if (settings.LogsChannelId == null || !settings.MessageEditLogsEnabled) return;

        // Check if the message is cached.
        MessageCache.TryGetValue(currentMessage.Id, out Message previousMessage);

        // Get the guild.
        var guild = await Client.GetOrFetchGuildAsync((ulong)currentMessage.GuildId!);
        if (guild is null) return;

        // Get the logs channel.
        var logsChannel = guild.Channels[settings.LogsChannelId.Value];
        if (logsChannel is null) return;


        // If previousMessage length + currentMessage length is more than 4000 (Max Discord Message Size), only include the old content.
        var description = $"**Old Content:**\n{previousMessage?.Content ?? "Unknown."}\n\n**[New Content]({currentMessage.GetUrl()}):**\n{currentMessage.Content}";
        if (((previousMessage?.Content.Length ?? 0) + currentMessage.Content.Length) >= 4000) {
            description = $"**Old Content:**\n{previousMessage?.Content ?? "Unknown."}\n\n**New Content:**\nClick [here]({currentMessage.GetUrl()})";
        }

        // Send the log message.
        await ((TextGuildChannel)logsChannel).SendMessageAsync(
            new MessageProperties().WithEmbeds(
                [
                    EmbedHelper.Embed(
                        title: "Message Edited",
                        description: description,
                        color: Colors.Yellow,
                        fields: [
                            EmbedHelper.Field("Author:", currentMessage.Author.ToString(), true),
                            EmbedHelper.Field("Channel:", currentMessage.Channel!.ToString(), true)
                        ],
                        thumbnail: new EmbedThumbnailProperties(currentMessage.Author.GetAvatarUrl()?.ToString()) ?? null,
                        timestamp: DateTimeOffset.UtcNow
                    )
                ]
            )
        );

        // Update the message in the cache.
        MessageCache.Update(currentMessage);
    }
}
