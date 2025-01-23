

using NetCord.Gateway;

internal static class MessageExtensions
{
    public static string GetUrl(this Message message)
    {
        return $"https://discord.com/channels/{message.GuildId}/{message.ChannelId}/{message.Id}";
    }
}