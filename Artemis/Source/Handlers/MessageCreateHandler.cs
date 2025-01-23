using NetCord.Gateway;
using NetCord.Hosting.Gateway;

namespace Handlers;

[GatewayEvent(nameof(GatewayClient.MessageCreate))]
public class MessageCreateHandler(Cache.MessageCache cache) : IGatewayEventHandler<Message>
{
    public readonly Cache.MessageCache MessageCache = cache;

    public ValueTask HandleAsync(Message message)
    {
        // Immediately cache all non-bot messages.
        if (!message.Author.IsBot) MessageCache.Add(message);

        return default;
    }
}
