using Artemis.DB.Models;
using NetCord;
using NetCord.Gateway;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using NetCord.Services.ComponentInteractions;

namespace Modules;

[SlashCommand(
    "voicemaster",
    "Commands for managing automatically created voice channels.",
    Contexts = [InteractionContextType.Guild],
    DefaultGuildUserPermissions = Permissions.ManageChannels
)]
public class VoiceMasterSetupModule : ApplicationCommandModule<ApplicationCommandContext>
{
    [SubSlashCommand("setup", "Set up VoiceMaster for this server.")]
    public async Task SetupAsync() {
        var embed = new EmbedProperties()
            .WithTitle("VoiceMaster Setup")
            .WithColor(Colors.Blue)
            .WithDescription("Please select one of the options below.")
            .AddFields(
                new EmbedFieldProperties()
                    .WithName(VoiceMasterButtonModule.EnableButton.Label)
                    .WithValue("Set the VoiceMaster channel and enable VoiceMaster functionality for this server.")
                    .WithInline(true),
                new EmbedFieldProperties()
                    .WithName(VoiceMasterButtonModule.DisableButton.Label)
                    .WithValue("Disable VoiceMaster functionality for this server.")
                    .WithInline(true)
            );

        var components = new ActionRowProperties()
            .WithButtons([
                VoiceMasterButtonModule.EnableButton,
                VoiceMasterButtonModule.DisableButton
            ]);

        await RespondAsync(
            InteractionCallback.Message(
                new InteractionMessageProperties()
                .WithEmbeds([embed])
                .WithComponents([components])
                .WithFlags(MessageFlags.Ephemeral)
            )
        );
    }
}

[SlashCommand("vc", "Commands for modifying your VoiceMaster channel settings.", Contexts = [InteractionContextType.Guild])]
public class VoiceMasterSettingsModule : ApplicationCommandModule<ApplicationCommandContext>
{
    public static readonly EmbedProperties SettingsEmbed = new EmbedProperties()
        .WithTitle("Your Channel Settings")
        .WithColor(Colors.Blue)
        .WithDescription("Please use the drop down menu below to update your settings.");

    // This embed is also sent when creating a VoiceMaster channel.
    [SubSlashCommand("settings", "Modify your VoiceMaster channel settings.")]
    public async Task SettingsAsync() {
        await RespondAsync(
            InteractionCallback.Message(
                new InteractionMessageProperties()
                .WithEmbeds([SettingsEmbed])
                .WithComponents([VoiceMasterStringMenuModule.SettingsMenu])
                .WithFlags(MessageFlags.Ephemeral)
            )
        );
    }
}

public class VoiceMasterButtonModule : ComponentInteractionModule<ButtonInteractionContext>
{
    public const string EnableButtonId = "voicemaster.setup.enable";
    public const string DisableButtonId = "voicemaster.setup.disable";
    public readonly static ButtonProperties EnableButton = new(EnableButtonId, "1️⃣", ButtonStyle.Secondary);
    public readonly static ButtonProperties DisableButton = new(DisableButtonId, "2️⃣", ButtonStyle.Secondary);

    [ComponentInteraction(EnableButtonId)]
    public async Task EnableAsync() {
        var embed = new EmbedProperties()
            .WithColor(Colors.Blue)
            .WithDescription("Please select the VoiceMaster channel below.");

        await RespondAsync(
            InteractionCallback.Message(
                new InteractionMessageProperties()
                .WithEmbeds([embed])
                .WithComponents([VoiceMasterChannelMenuModule.EnableMenu])
                .WithFlags(MessageFlags.Ephemeral)
            )
        );
    }

    [ComponentInteraction(DisableButtonId)]
    public async Task DisableAsync() {
        GuildSettings.Upsert(Context.Guild!.Id, x => x.VoiceMasterChannelId = null);

        await RespondAsync(
            InteractionCallback.Message(
                new InteractionMessageProperties()
                .WithEmbeds([
                    new EmbedProperties()
                    .WithColor(Colors.Green)
                    .WithDescription("VoiceMaster functionality has been disabled for this server.")
                ])
                .WithFlags(MessageFlags.Ephemeral)
            )
        );
    }
}

public class VoiceMasterModalModule : ComponentInteractionModule<ModalInteractionContext>
{
    public const string SetNameModalId = "vc.settings.setname.modal";
    public const string SetNameInputId = "vc.settings.setname.modal.input";
    public const string SetLimitModalId = "vc.settings.setlimit.modal";
    public const string SetLimitInputId = "vc.settings.setlimit.modal.input";
    public static readonly ModalProperties SetNameModal = new(
        SetNameModalId, "Set Name",
        [new TextInputProperties(SetNameInputId, TextInputStyle.Short, "Name").WithMaxLength(40)]
        );
    public static readonly ModalProperties SetLimitModal = new(
        SetLimitModalId, "Set User Limit",
        [new TextInputProperties(SetLimitInputId, TextInputStyle.Short, "Limit").WithMaxLength(2).WithMinLength(1)]
        );

    [ComponentInteraction(SetNameModalId)]
    public async Task SetNameModalAsync() {
        await RespondAsync(InteractionCallback.DeferredModifyMessage);
        var input = Context.Components.OfType<TextInput>().First().Value;
        await HandleModalAsync(input, isName: true);
    }

    [ComponentInteraction(SetLimitModalId)]
    public async Task SetLimitModalAsync() {
        await RespondAsync(InteractionCallback.DeferredModifyMessage);
        var inputValue = Context.Components.OfType<TextInput>().First().Value;
        if (!int.TryParse(inputValue, out var input)) {
            await ModifyResponseAsync(msg => msg.Components = [VoiceMasterStringMenuModule.SettingsMenu]);
            await FollowupAsync(new InteractionMessageProperties()
                .WithEmbeds([
                    new EmbedProperties()
                    .WithColor(Colors.Red)
                    .WithDescription($"Invalid input. Please enter a number between 0 and 99.")
                ])
                .WithFlags(MessageFlags.Ephemeral)
                );
            return;
        }
        await HandleModalAsync(inputValue, isName: false);
    }

    private async Task HandleModalAsync(string input, bool isName) {
        if (!UserVoiceMasterSettings.TryGet(Context.User!.Id, out var userSettings)) {
            userSettings = UserVoiceMasterSettings.Default(Context.User!);
            UserVoiceMasterSettings.Upsert(userSettings);
        }
        var guildVmChannels = GuildVoiceMasterChannels.GetOrCreate(Context.Guild!.Id);

        // Change the user's settings.
        if (isName) {
            userSettings.ChannelName = input;
        }
        else {
            userSettings.ChannelLimit = int.Parse(input);
        }
        UserVoiceMasterSettings.Upsert(userSettings);

        var guild = await Context.Client.GetOrFetchGuildAsync(Context.Guild!.Id);
        var guildUser = await guild!.GetUserAsync(Context.User!.Id)!;
        VoiceState? userVoiceState = null;

        try {
            userVoiceState = await guildUser.GetVoiceStateAsync();
        }
        catch { }

        // If the user is in a channel, check if its a VoiceMaster channel and that they own it.
        var userOwnsChannel = false;
        if (userVoiceState != null) {
            if (guildVmChannels.Channels.TryGetValue((ulong)userVoiceState.ChannelId!, out var userId)) {
                userOwnsChannel = userId == Context.User!.Id;
            }
        }

        if (userVoiceState == null || !userOwnsChannel) {
            await ModifyResponseAsync(msg => msg.Components = [VoiceMasterStringMenuModule.SettingsMenu]);
            await FollowupAsync(new InteractionMessageProperties()
                .WithEmbeds([
                    new EmbedProperties()
                    .WithColor(Colors.Green)
                    .WithDescription(isName
                        ? $"Your VoiceMaster channel name has been set to **{input}**. This will apply the next time your channel is created."
                        : $"Your VoiceMaster channel limit has been set to **{input}**. This will apply the next time your channel is created.")
                ])
                .WithFlags(MessageFlags.Ephemeral)
                );
        }
        else if (userOwnsChannel) {
            // Get the user's channel and update the name or limit.
            guild!.Channels.TryGetValue((ulong)userVoiceState.ChannelId!, out var channel);
            if (channel != null) {
                if (isName) {
                    await channel.ModifyAsync(x => x.Name = input);
                }
                else {
                    await channel.ModifyAsync(x => x.UserLimit = int.Parse(input));
                }
            }

            await ModifyResponseAsync(msg => msg.Components = [VoiceMasterStringMenuModule.SettingsMenu]);
            await FollowupAsync(new InteractionMessageProperties()
                .WithEmbeds([
                    new EmbedProperties()
                    .WithColor(Colors.Green)
                    .WithDescription(isName
                        ? $"Your VoiceMaster channel name has been set to **{input}**."
                        : $"Your VoiceMaster channel limit has been set to **{input}**.")
                ])
                .WithFlags(MessageFlags.Ephemeral)
                );
        }
    }
}

public class VoiceMasterStringMenuModule : ComponentInteractionModule<StringMenuInteractionContext>
{
    public const string SettingsMenuId = "vc.settings.select";
    public const string SetNameOptionId = "vc.settings.setname";
    public const string SetLimitOptionId = "vc.settings.setlimit";
    public static readonly StringMenuProperties SettingsMenu = new StringMenuProperties(SettingsMenuId).AddOptions([
            new StringMenuSelectOptionProperties("Set Name", SetNameOptionId).WithDescription("Set the name of your VoiceMaster channel."),
            new StringMenuSelectOptionProperties("Set User Limit", SetLimitOptionId).WithDescription("Set the user limit of your VoiceMaster channel."),
    ]);

    [ComponentInteraction(SettingsMenuId)]
    public async Task SettingsMenuAsync() {
        var option = Context.SelectedValues[0];
        switch (option) {
            case SetNameOptionId:
                await RespondAsync(InteractionCallback.Modal(VoiceMasterModalModule.SetNameModal));
                break;
            case SetLimitOptionId:
                await RespondAsync(InteractionCallback.Modal(VoiceMasterModalModule.SetLimitModal));
                break;
        }
    }
}

public class VoiceMasterChannelMenuModule : ComponentInteractionModule<ChannelMenuInteractionContext>
{
    public const string EnableMenuId = "voicemaster.setup.enable.select";
    public static readonly ChannelMenuProperties EnableMenu = new ChannelMenuProperties(EnableMenuId).AddChannelTypes([ChannelType.VoiceGuildChannel]);

    [ComponentInteraction(EnableMenuId)]
    public async Task EnableChannelMenuAsync() {
        await RespondAsync(InteractionCallback.DeferredModifyMessage);

        var channel = Context.SelectedChannels[0]; // There should only ever be one channel selected.
        GuildSettings.Upsert(Context.Guild!.Id, x => x.VoiceMasterChannelId = channel.Id);

        await ModifyResponseAsync(msg => {
            msg.Components = [];
            msg.Embeds = [
                new EmbedProperties()
                .WithColor(Colors.Green)
                .WithDescription($"VoiceMaster channel has been set to <#{channel.Id}>.")
            ];
        });
    }
}
