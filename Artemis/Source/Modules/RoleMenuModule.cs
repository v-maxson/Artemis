using Artemis.DB.Models;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using NetCord.Services.ComponentInteractions;
using Serilog;

namespace Modules;

[SlashCommand(
    "rolemenu",
    "Commands for managing role menus.",
    Contexts = [InteractionContextType.Guild],
    DefaultGuildUserPermissions = Permissions.ManageRoles
)]
public partial class RoleMenuModule : ApplicationCommandModule<ApplicationCommandContext>
{
    [SubSlashCommand("setup", "Setup and modify role menus.")]
    public async Task SetupAsync() {
        var embed = EmbedHelper.Embed(
            title: "Role Menu Setup",
            color: Colors.Pink,
            description: "Please select one of the options below.",
            fields: [
                EmbedHelper.Field(RoleMenuButtonModule.CreateRoleButton.Label, "Create a new role menu."),
                EmbedHelper.Field(RoleMenuButtonModule.ModifyRoleButton.Label, "Modify an existing role menu."),
                EmbedHelper.Field(RoleMenuButtonModule.DeleteRoleButton.Label, "Delete a role menu.")
            ]
        );

        var components = new ActionRowProperties()
            .WithButtons([
                RoleMenuButtonModule.CreateRoleButton,
                RoleMenuButtonModule.ModifyRoleButton,
                RoleMenuButtonModule.DeleteRoleButton
            ]);

        await RespondAsync(InteractionCallback.Message(
            new InteractionMessageProperties()
            .WithEmbeds([embed])
            .WithComponents([components])
            .WithFlags(MessageFlags.Ephemeral)
            ));
    }

    #region Send Metadata
    [SubSlashCommand("send", "Send a role menu.")]
    public partial Task SendAsync(
        [SlashCommandParameter(Name = "role_menu_name", Description = "The name of the role menu to send.", AutocompleteProviderType = typeof(RoleMenuAutocompleteProvider))]
        string roleMenuName,

        [SlashCommandParameter(Name = "channel", Description = "The channel to send the role menu to. Default: Current Channel")]
        TextGuildChannel? channel = null
    );
    #endregion
    public partial async Task SendAsync(string roleMenuName, TextGuildChannel? channel) {
        await RespondAsync(InteractionCallback.DeferredMessage(MessageFlags.Ephemeral));

        if (!GuildRoleMenus.TryGet(Context.Guild!.Id, out var guildRoleMenus)) {
            await ModifyResponseAsync(msg => {
                msg.Embeds = [
                    EmbedHelper.Embed(
                        color: Colors.Red,
                        description: "There are no Role Menus to send."
                    )
                ];
            });
            return;
        }

        if (!guildRoleMenus.RoleMenus.TryGetValue(roleMenuName, out GuildRoleMenus.RoleMenu? roleMenu)) {
            await ModifyResponseAsync(msg => {
                msg.Embeds = [
                    EmbedHelper.Embed(
                        color: Colors.Red,
                        description: "That Role Menu does not exist."
                    )
                ];
            });
            return;
        }

        if (roleMenu.RoleIds.Count == 0) {
            await ModifyResponseAsync(msg => {
                msg.Embeds = [
                    EmbedHelper.Embed(
                        color: Colors.Red,
                        description: "That Role Menu does not have any roles."
                    )
                ];
            });
            return;
        }

        var description = roleMenu.MultiSelect ? "Please select 1 or more roles using the drop down menu below." : "Please select a role using the drop down menu below.";
        var roles = roleMenu.RoleIds.Select(id => Context.Guild!.Roles[id]).ToList();

        var embed = EmbedHelper.Embed(
            title: roleMenuName,
            description: description,
            color: Colors.Pink
        );

        var components = new StringMenuProperties(RoleMenuStringMenuModule.GenerateRoleSelectMenuId(roleMenuName, roleMenu.MultiSelect))
            .WithOptions(roles.Select(role => new StringMenuSelectOptionProperties(role.Name, role.Id.ToString())));

        if (roleMenu.MultiSelect)
            components.WithMinValues(0).WithMaxValues(roles.Count);

        var finalChannel = channel ?? Context.Channel;
        await finalChannel.SendMessageAsync(new MessageProperties().WithEmbeds([embed]).WithComponents([components]));

        await ModifyResponseAsync(msg => {
            msg.Embeds = [
                EmbedHelper.Embed(
                    color: Colors.Pink,
                    description: $"Role Menu '{roleMenuName}' has been sent to {finalChannel}."
                )
            ];
        });
    }

    public static (EmbedProperties, IEnumerable<ComponentProperties>) GenerateMenuEditor(string name, IEnumerable<ulong> includedRoles, bool isMultiSelect) {
        var roles = includedRoles.Any() ? string.Join(", ", includedRoles.Select(id => $"<@&{id}>")) : "*None*";
        var multiSelect = isMultiSelect ? "Yes, this menu can be used to apply any of the above roles." : "No, this menu can only be used to apply one of the above roles.";

        var embed = EmbedHelper.Embed(
            title: $"'{name}' Role Menu Editor",
            description: "Please use the drop down menus below to add/remove roles from this menu.",
            color: Colors.Pink,
            fields: [
                EmbedHelper.Field("Include Roles:", roles),
                EmbedHelper.Field("Multi-Select:", multiSelect)
            ]
        );

        List<ComponentProperties> components = [
            new ActionRowProperties()
                .WithButtons([
                    new ButtonProperties(RoleMenuButtonModule.GenerateToggleMultiSelectId(name), "Toggle Multi-Select", ButtonStyle.Secondary)
                ]
            ),
            new RoleMenuProperties(RoleMenuRoleMenuModule.GenerateRoleSelectMenuId(name)).WithMaxValues(25).WithMinValues(0).WithPlaceholder("Select up to 25 roles."),
        ];

        return (embed, components);
    }
}

public class RoleMenuAutocompleteProvider : IAutocompleteProvider<AutocompleteInteractionContext>
{
    public ValueTask<IEnumerable<ApplicationCommandOptionChoiceProperties>?> GetChoicesAsync(ApplicationCommandInteractionDataOption option, AutocompleteInteractionContext context) {
        var input = option.Value!;

        if (!GuildRoleMenus.TryGet(context.Guild!.Id, out var guildRoleMenus))
            return new();

        var result = guildRoleMenus.RoleMenus.Keys
            .Where(x => x.Contains(input, StringComparison.OrdinalIgnoreCase))
            .Select(x => new ApplicationCommandOptionChoiceProperties(x, x));

        return new(result);
    }
}

public class RoleMenuButtonModule : ComponentInteractionModule<ButtonInteractionContext>
{
    public const string CreateRoleMenuId = "rolemenu.createrolemenu";
    public const string ModifyRoleMenuId = "rolemenu.modifyrolemenu";
    public const string DeleteRoleMenuId = "rolemenu.deleterolemenu";
    public const string ToggleMultiSelectId = $"rolemenu.menueditor.togglemultiselect";

    public static string GenerateToggleMultiSelectId(string name) => $"{ToggleMultiSelectId}:{name}";

    public static readonly ButtonProperties CreateRoleButton = new(CreateRoleMenuId, "1️⃣", ButtonStyle.Primary);
    public static readonly ButtonProperties ModifyRoleButton = new(ModifyRoleMenuId, "2️⃣", ButtonStyle.Primary);
    public static readonly ButtonProperties DeleteRoleButton = new(DeleteRoleMenuId, "❌", ButtonStyle.Secondary);

    [ComponentInteraction(CreateRoleMenuId)]
    public async Task CreateRoleMenuAsync() {
        await RespondAsync(InteractionCallback.Modal(RoleMenuModalModule.CreateRoleMenuModal));
    }

    [ComponentInteraction(ModifyRoleMenuId)]
    public async Task ModifyRoleMenuAsync() {
        await RespondAsync(InteractionCallback.DeferredMessage(MessageFlags.Ephemeral));

        if (!GuildRoleMenus.TryGet(Context.Guild!.Id, out var guildRoleMenus) || guildRoleMenus.RoleMenus.Count == 0) {
            await ModifyResponseAsync(msg => {
                msg.Embeds = [
                    EmbedHelper.Embed(
                        color: Colors.Red,
                        description: "There are no Role Menus to modify."
                    )
                ];
            });
            return;
        }

        await ModifyResponseAsync(msg => {
            msg.Embeds = [
                EmbedHelper.Embed(
                    color: Colors.Pink,
                    description: "Please select a Role Menu to modify."
                )
            ];

            msg.Components = [
                new StringMenuProperties(RoleMenuStringMenuModule.ModifyMenuId)
                .AddOptions(guildRoleMenus.RoleMenus.Keys.Select(x => new StringMenuSelectOptionProperties(x, x)))
                ];
        });
    }

    [ComponentInteraction(DeleteRoleMenuId)]
    public async Task DeleteRoleMenuAsync() {
        await RespondAsync(InteractionCallback.DeferredMessage(MessageFlags.Ephemeral));

        if (!GuildRoleMenus.TryGet(Context.Guild!.Id, out var guildRoleMenus) || guildRoleMenus.RoleMenus.Count == 0) {
            await ModifyResponseAsync(msg => {
                msg.Embeds = [
                    EmbedHelper.Embed(
                        color: Colors.Red,
                        description: "There are no Role Menus to delete."
                    )
                ];
            });
            return;
        }

        await ModifyResponseAsync(msg => {
            msg.Embeds = [
                EmbedHelper.Embed(
                    color: Colors.Pink,
                    description: "Please select a Role Menu to delete."
                )
            ];

            msg.Components = [
                new StringMenuProperties(RoleMenuStringMenuModule.DeleteMenuId)
                .AddOptions(guildRoleMenus.RoleMenus.Keys.Select(x => new StringMenuSelectOptionProperties(x, x)))
                ];
        });
    }

    [ComponentInteraction(ToggleMultiSelectId)]
    public async Task ToggleMultiSelectAsync(string name) {
        await RespondAsync(InteractionCallback.DeferredModifyMessage);

        var updatedMenu = GuildRoleMenus.Upsert(Context.Guild!.Id, roleMenus => {
            roleMenus.RoleMenus[name].MultiSelect = !roleMenus.RoleMenus[name].MultiSelect;
        });

        await ModifyResponseAsync(msg => {
            var menuEditor = RoleMenuModule.GenerateMenuEditor(name, updatedMenu.RoleMenus[name].RoleIds, updatedMenu.RoleMenus[name].MultiSelect);
            msg.Embeds = [menuEditor.Item1];
            msg.Components = menuEditor.Item2;
        });
    }
}

public class RoleMenuModalModule : ComponentInteractionModule<ModalInteractionContext>
{
    public const string CreateRoleMenuModalId = $"{RoleMenuButtonModule.CreateRoleMenuId}.modal";
    public const string CreateRoleMenuNameInputId = $"{RoleMenuButtonModule.CreateRoleMenuId}.modal.name";
    public static readonly ModalProperties CreateRoleMenuModal = new(
        CreateRoleMenuModalId, "Please input a name.",
        [new TextInputProperties(CreateRoleMenuNameInputId, TextInputStyle.Short, "Name").WithMaxLength(50)]);

    [ComponentInteraction(CreateRoleMenuModalId)]
    public async Task CreateRoleMenuModalAsync() {
        var name = Context.Components.OfType<TextInput>().First().Value;

        // Create a new role menu with the given name.
        var roleMenus = GuildRoleMenus.GetOrCreate(Context.Guild!.Id);

        if (roleMenus.RoleMenus.ContainsKey(name)) {
            await RespondAsync(InteractionCallback.Message(
                new InteractionMessageProperties()
                .WithEmbeds([
                    EmbedHelper.Embed(
                        color: Colors.Red,
                        description: "A role menu with that name already exists."
                    )
                ])
                .WithFlags(MessageFlags.Ephemeral)
                ));
            return;
        }

        roleMenus.RoleMenus.Add(name, new());
        GuildRoleMenus.Upsert(roleMenus);

        var menuEditor = RoleMenuModule.GenerateMenuEditor(name, roleMenus.RoleMenus[name].RoleIds, roleMenus.RoleMenus[name].MultiSelect);

        await RespondAsync(InteractionCallback.Message(
            new InteractionMessageProperties()
            .WithEmbeds([menuEditor.Item1])
            .WithComponents(menuEditor.Item2)
            .WithFlags(MessageFlags.Ephemeral)
            ));

    }
}

public class RoleMenuRoleMenuModule : ComponentInteractionModule<RoleMenuInteractionContext>
{
    public const string RoleSelectMenuId = "rolemenu.menueditor.addroles";

    public static string GenerateRoleSelectMenuId(string name) => $"{RoleSelectMenuId}:{name}";

    [ComponentInteraction(RoleSelectMenuId)]
    public async Task RoleSelectMenuAsync(string name) {
        // Check if any of the selected roles are managed by an integration.
        if (Context.SelectedRoles.Any(x => x.Managed)) {
            await RespondAsync(InteractionCallback.Message(
                new InteractionMessageProperties()
                .WithEmbeds([
                    EmbedHelper.Embed(
                        color: Colors.Red,
                        description: "You cannot add integration-managed roles (bot roles) to a role menu."
                    )
                ])
                .WithFlags(MessageFlags.Ephemeral)
            ));
            return;
        }

        await RespondAsync(InteractionCallback.DeferredModifyMessage);

        var updatedMenu = GuildRoleMenus.Upsert(Context.Guild!.Id, roleMenus => {
            roleMenus.RoleMenus[name].RoleIds = Context.SelectedRoles.Select(x => x.Id).ToList();
        });

        await ModifyResponseAsync(msg => {
            var menuEditor = RoleMenuModule.GenerateMenuEditor(name, updatedMenu.RoleMenus[name].RoleIds, updatedMenu.RoleMenus[name].MultiSelect);
            msg.Embeds = [menuEditor.Item1];
            msg.Components = menuEditor.Item2;
        });
    }
}

public class RoleMenuStringMenuModule : ComponentInteractionModule<StringMenuInteractionContext>
{
    public const string ModifyMenuId = $"{RoleMenuButtonModule.ModifyRoleMenuId}.select";
    public const string DeleteMenuId = $"{RoleMenuButtonModule.DeleteRoleMenuId}.select";
    public const string RoleSelectMenuId = "rolemenu.menu.selectroles";

    public static string GenerateRoleSelectMenuId(string name, bool isMultiSelect) => $"{RoleSelectMenuId}:{name}:{isMultiSelect}";

    [ComponentInteraction(RoleSelectMenuId)]
    public async Task RoleSelectAsync(string name, bool isMultiSelect) {
        await RespondAsync(InteractionCallback.DeferredMessage(MessageFlags.Ephemeral));

        var selectedRoles = Context.SelectedValues.Select(ulong.Parse).ToList();

        GuildRoleMenus.TryGet(Context.Guild!.Id, out var guildRoleMenus);
        var menu = guildRoleMenus?.RoleMenus[name];
        var menuRoles = menu?.RoleIds.Select(id => Context.Guild!.Roles[id]).ToList();
        var member = await Context.Guild.GetUserAsync(Context.User.Id);

        if (!isMultiSelect) {
            var selectedRole = selectedRoles[0];
            // Add the selected role to the user. And remove any other roles in the menu.
            var rolesToRemove = menuRoles?.Where(x => x.Id != selectedRole).Select(x => x.Id).ToList();

            // Add the selected role and remove the others.
            _ = member.AddRoleAsync(selectedRole);

            if (rolesToRemove?.Count > 0) {
                foreach (var role in rolesToRemove) {
                    if (member.RoleIds.Contains(role))
                        _ = member.RemoveRoleAsync(role);
                }
            }
        }
        else {
            // Add the selected roles to the user. And remove any other roles in the menu.
            var rolesToAdd = selectedRoles.Where(x => !member.RoleIds.Contains(x)).ToList();
            var rolesToRemove = menuRoles?.Where(x => !selectedRoles.Contains(x.Id)).Select(x => x.Id).ToList();

            // Add the selected roles.
            if (rolesToAdd.Count > 0) {
                foreach (var role in rolesToAdd) {
                    if (!member.RoleIds.Contains(role))
                        _ = member.AddRoleAsync(role);
                }
            }

            // Remove the unselected roles.
            if (rolesToRemove?.Count > 0) {
                foreach (var role in rolesToRemove) {
                    if (member.RoleIds.Contains(role))
                        _ = member.RemoveRoleAsync(role);
                }
            }

        }

        await ModifyResponseAsync(msg => {
            msg.Embeds = [
                EmbedHelper.Embed(
                    color: Colors.Pink,
                    description: "Roles have been added/removed."
                )
            ];
        });
    }

    [ComponentInteraction(ModifyMenuId)]
    public async Task ModifyMenuAsync() {
        await RespondAsync(InteractionCallback.DeferredModifyMessage);

        var selectedMenuName = Context.SelectedValues[0];

        GuildRoleMenus.TryGet(Context.Guild!.Id, out var guildRoleMenus);
        var menu = guildRoleMenus?.RoleMenus[selectedMenuName];

        await ModifyResponseAsync(msg => {
            var menuEditor = RoleMenuModule.GenerateMenuEditor(selectedMenuName, menu!.RoleIds, menu!.MultiSelect);
            msg.Embeds = [menuEditor.Item1];
            msg.Components = menuEditor.Item2;
        });
    }

    [ComponentInteraction(DeleteMenuId)]
    public async Task DeleteMenuAsync() {
        await RespondAsync(InteractionCallback.DeferredModifyMessage);

        var selectedMenuName = Context.SelectedValues[0];

        var updatedMenu = GuildRoleMenus.Upsert(Context.Guild!.Id, roleMenus => {
            roleMenus.RoleMenus.Remove(selectedMenuName);
        });

        await ModifyResponseAsync(msg => {
            msg.Embeds = [
                EmbedHelper.Embed(
                    color: Colors.Pink,
                    description: $"Role Menu '{selectedMenuName}' has been deleted."
                )
            ];
            msg.Components = [];
        });
    }
}
