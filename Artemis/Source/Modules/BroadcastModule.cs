using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using static Colors;

namespace Modules;

public partial class BroadcastModule : ApplicationCommandModule<ApplicationCommandContext>
{
    #region Broadcast Metadata
    [SlashCommand(
        "broadcast",
        "Broadcast a message.",
        Contexts = [InteractionContextType.Guild],
        DefaultGuildUserPermissions = Permissions.ManageGuild
    )]
    public partial Task BroadcastAsync(
        [SlashCommandParameter(Name = "message", Description = "The message to broadcast.")]
        string message,

        [SlashCommandParameter(Name = "title", Description = "The title of the broadcast.")]
        string? title = null,

        [SlashCommandParameter(Name = "mention", Description = "The role to mention. Default: @everyone")]
        Role? mention = null,

        [SlashCommandParameter(Name = "channel", Description = "The channel to broadcast in. Default: current channel.")]
        TextGuildChannel? channel = null,

        [SlashCommandParameter(Name = "color", Description = "The color of the embed. Default: Pink")]
        ColorEnum color = ColorEnum.Pink
    );
    #endregion
    public partial async Task BroadcastAsync(string message, string? title, Role? mention, TextGuildChannel? channel, ColorEnum color) {
        await RespondAsync(InteractionCallback.DeferredMessage(MessageFlags.Ephemeral));

        var finalMention = mention is null || mention.Position == 0 || mention.Managed ? "@everyone" : $"<@{mention.Id}>";
        var finalChannel = channel is null ? Context.Channel : channel;

        var footerProperties = new EmbedFooterProperties()
            .WithText($"Broadcasted by {Context.User.Username}")
            .WithIconUrl(Context.User.GetAvatarUrl()!.ToString());

        var embed = new EmbedProperties()
            .WithDescription(message)
            .WithColor(new Color((int)color))
            .WithFooter(new EmbedFooterProperties()
                .WithText($"Broadcasted by {Context.User.Username}")
                .WithIconUrl(Context.User.GetAvatarUrl()!.ToString()
            ));

        if (title != null) embed.WithTitle(title);

        await finalChannel.SendMessageAsync(new MessageProperties().WithContent(finalMention).WithEmbeds([embed]));
        await ModifyResponseAsync(msg => {
            msg.Embeds = [new EmbedProperties().WithDescription("Message Broadcasted!")];
        });
    }
}

