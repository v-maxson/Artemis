using LiteDB;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace Modules;

[SlashCommand("nwdb", "New World Database commands.")]
public partial class NewWorldDatabaseModule : ApplicationCommandModule<ApplicationCommandContext>
{
    #region Search Commands
    [SubSlashCommand("item", "Search for an item in the New World Database.")]
    public async Task ItemAsync(
        [SlashCommandParameter(Name = "name", Description = "The name of the item to search for.", AutocompleteProviderType = typeof(ItemsAutocompleteProvider))]
        string name,

        [SlashCommandParameter(Name = "ephemeral", Description = "Whether the response should be ephemeral (only displays for you). Default: True")]
        bool ephemeral = true
    ) => await SendLink(name, ephemeral);

    [SubSlashCommand("mount", "Search for a mount in the New World Database.")]
    public async Task MountAsync(
        [SlashCommandParameter(Name = "name", Description = "The name of the mount to search for.", AutocompleteProviderType = typeof(MountsAutocompleteProvider))]
        string name,

        [SlashCommandParameter(Name = "ephemeral", Description = "Whether the response should be ephemeral (only displays for you). Default: True")]
        bool ephemeral = true
    ) => await SendLink(name, ephemeral);

    [SubSlashCommand("recipe", "Search for a recipe in the New World Database.")]
    public async Task RecipeAsync(
        [SlashCommandParameter(Name = "name", Description = "The name of the recipe to search for.", AutocompleteProviderType = typeof(RecipesAutocompleteProvider))]
        string name,

        [SlashCommandParameter(Name = "ephemeral", Description = "Whether the response should be ephemeral (only displays for you). Default: True")]
        bool ephemeral = true
    ) => await SendLink(name, ephemeral);

    [SubSlashCommand("ability", "Search for an ability in the New World Database.")]
    public async Task AbilityAsync(
        [SlashCommandParameter(Name = "name", Description = "The name of the ability to search for.", AutocompleteProviderType = typeof(AbilitiesAutocompleteProvider))]
        string name,

        [SlashCommandParameter(Name = "ephemeral", Description = "Whether the response should be ephemeral (only displays for you). Default: True")]
        bool ephemeral = true
    ) => await SendLink(name, ephemeral);

    [SubSlashCommand("perk", "Search for a perk in the New World Database.")]
    public async Task PerkAsync(
        [SlashCommandParameter(Name = "name", Description = "The name of the perk to search for.", AutocompleteProviderType = typeof(PerksAutocompleteProvider))]
        string name,

        [SlashCommandParameter(Name = "ephemeral", Description = "Whether the response should be ephemeral (only displays for you). Default: True")]
        bool ephemeral = true
    ) => await SendLink(name, ephemeral);

    [SubSlashCommand("status-effect", "Search for a status effect in the New World Database.")]
    public async Task StatusEffectAsync(
        [SlashCommandParameter(Name = "name", Description = "The name of the status effect to search for.", AutocompleteProviderType = typeof(StatusEffectsAutocompleteProvider))]
        string name,

        [SlashCommandParameter(Name = "ephemeral", Description = "Whether the response should be ephemeral (only displays for you). Default: True")]
        bool ephemeral = true
    ) => await SendLink(name, ephemeral);

    [SubSlashCommand("quest", "Search for a quest in the New World Database.")]
    public async Task QuestAsync(
        [SlashCommandParameter(Name = "name", Description = "The name of the quest to search for.", AutocompleteProviderType = typeof(QuestsAutocompleteProvider))]
        string name,

        [SlashCommandParameter(Name = "ephemeral", Description = "Whether the response should be ephemeral (only displays for you). Default: True")]
        bool ephemeral = true
    ) => await SendLink(name, ephemeral);

    [SubSlashCommand("creature", "Search for a creature in the New World Database.")]
    public async Task CreatureAsync(
        [SlashCommandParameter(Name = "name", Description = "The name of the creature to search for.", AutocompleteProviderType = typeof(CreaturesAutocompleteProvider))]
        string name,

        [SlashCommandParameter(Name = "ephemeral", Description = "Whether the response should be ephemeral (only displays for you). Default: True")]
        bool ephemeral = true
    ) => await SendLink(name, ephemeral);

    [SubSlashCommand("gatherable", "Search for a gatherable in the New World Database.")]
    public async Task GatherableAsync(
        [SlashCommandParameter(Name = "name", Description = "The name of the gatherable to search for.", AutocompleteProviderType = typeof(GatherablesAutocompleteProvider))]
        string name,

        [SlashCommandParameter(Name = "ephemeral", Description = "Whether the response should be ephemeral (only displays for you). Default: True")]
        bool ephemeral = true
    ) => await SendLink(name, ephemeral);

    [SubSlashCommand("shop", "Search for a shop in the New World Database.")]
    public async Task ShopAsync(
        [SlashCommandParameter(Name = "name", Description = "The name of the shop to search for.", AutocompleteProviderType = typeof(ShopsAutocompleteProvider))]
        string name,

        [SlashCommandParameter(Name = "ephemeral", Description = "Whether the response should be ephemeral (only displays for you). Default: True")]
        bool ephemeral = true
    ) => await SendLink(name, ephemeral);

    [SubSlashCommand("npc", "Search for an NPC in the New World Database.")]
    public async Task NPCAsync(
        [SlashCommandParameter(Name = "name", Description = "The name of the NPC to search for.", AutocompleteProviderType = typeof(NPCSAutocompleteProvider))]
        string name,

        [SlashCommandParameter(Name = "ephemeral", Description = "Whether the response should be ephemeral (only displays for you). Default: True")]
        bool ephemeral = true
    ) => await SendLink(name, ephemeral);

    [SubSlashCommand("zone", "Search for a zone in the New World Database.")]
    public async Task ZoneAsync(
        [SlashCommandParameter(Name = "name", Description = "The name of the zone to search for.", AutocompleteProviderType = typeof(ZonesAutoCompleteProvider))]
        string name,

        [SlashCommandParameter(Name = "ephemeral", Description = "Whether the response should be ephemeral (only displays for you). Default: True")]
        bool ephemeral = true
    ) => await SendLink(name, ephemeral);
    #endregion

    [SubSlashCommand("count", "Count the number of items in the New World Database.")]
    public async Task CountAsync()
    {
        await RespondAsync(InteractionCallback.DeferredMessage(MessageFlags.Ephemeral));

        using var db = new LiteDatabase("nwdb.db");
        var collections = db.GetCollectionNames();
        
        // Get each collection.
        var items = db.GetCollection<NWDB.Entity>("items").Count();
        var mounts = db.GetCollection<NWDB.Entity>("mounts").Count();
        var recipes = db.GetCollection<NWDB.Entity>("recipes").Count();
        var abilities = db.GetCollection<NWDB.Entity>("abilities").Count();
        var perks = db.GetCollection<NWDB.Entity>("perks").Count();
        var statusEffects = db.GetCollection<NWDB.Entity>("statuseffects").Count();
        var quests = db.GetCollection<NWDB.Entity>("quests").Count();
        var creatures = db.GetCollection<NWDB.Entity>("creatures").Count();
        var gatherables = db.GetCollection<NWDB.Entity>("gatherables").Count();
        var shops = db.GetCollection<NWDB.Entity>("shops").Count();
        var npcs = db.GetCollection<NWDB.Entity>("npcs").Count();
        var zones = db.GetCollection<NWDB.Entity>("zones").Count();
        var total = collections.Sum(c => db.GetCollection<NWDB.Entity>(c).Count());

        await ModifyResponseAsync(msg =>
        {
            msg.Embeds = [
                new EmbedProperties()
                .WithTitle("New World Database Counts")
                .WithColor(Colors.Pink)
                .WithDescription(
                $"**Items**: {items}\n" +
                $"**Mounts**: {mounts}\n" +
                $"**Recipes**: {recipes}\n" +
                $"**Abilities**: {abilities}\n" +
                $"**Perks**: {perks}\n" +
                $"**Status Effects**: {statusEffects}\n" +
                $"**Quests**: {quests}\n" +
                $"**Creatures**: {creatures}\n" +
                $"**Gatherables**: {gatherables}\n" +
                $"**Shops**: {shops}\n" +
                $"**NPCs**: {npcs}\n" +
                $"**Zones**: {zones}\n\n" +
                $"**Total**: {total}")
                    ];
        });
    }

    private async Task SendLink(string link, bool ephemeral)
    {
        var message = new InteractionMessageProperties().WithContent($"[Here!]({link})");
        if (ephemeral) message.WithFlags(MessageFlags.Ephemeral);

        await RespondAsync(InteractionCallback.Message(message));
    }
}

#region AutocompleteProviders
static class NWDBAutoCompleteProvider
{
    private static readonly LiteDatabase DB = new("nwdb.db");

    public static ValueTask<IEnumerable<ApplicationCommandOptionChoiceProperties>?> GetChoicesAsync(
        ApplicationCommandInteractionDataOption option,
        string collectionName)
    {
        var input = option.Value!;

        var items = DB.GetCollection<NWDB.Entity>(collectionName);
        var result = items.Find(d => d.Name.Contains(input), limit: 25).Take(25)
            .Select(d => new ApplicationCommandOptionChoiceProperties(d.Name, d.Url));

        return new(result);
    }
}

public class ItemsAutocompleteProvider : IAutocompleteProvider<AutocompleteInteractionContext>
{
    public ValueTask<IEnumerable<ApplicationCommandOptionChoiceProperties>?> GetChoicesAsync(ApplicationCommandInteractionDataOption option, AutocompleteInteractionContext context)
    {
        return NWDBAutoCompleteProvider.GetChoicesAsync(option, "items");
    }
}

public class MountsAutocompleteProvider : IAutocompleteProvider<AutocompleteInteractionContext>
{
    public ValueTask<IEnumerable<ApplicationCommandOptionChoiceProperties>?> GetChoicesAsync(ApplicationCommandInteractionDataOption option, AutocompleteInteractionContext context)
    {
        return NWDBAutoCompleteProvider.GetChoicesAsync(option, "mounts");
    }
}

public class RecipesAutocompleteProvider : IAutocompleteProvider<AutocompleteInteractionContext>
{
    public ValueTask<IEnumerable<ApplicationCommandOptionChoiceProperties>?> GetChoicesAsync(ApplicationCommandInteractionDataOption option, AutocompleteInteractionContext context)
    {
        return NWDBAutoCompleteProvider.GetChoicesAsync(option, "recipes");
    }
}

public class AbilitiesAutocompleteProvider : IAutocompleteProvider<AutocompleteInteractionContext>
{
    public ValueTask<IEnumerable<ApplicationCommandOptionChoiceProperties>?> GetChoicesAsync(ApplicationCommandInteractionDataOption option, AutocompleteInteractionContext context)
    {
        return NWDBAutoCompleteProvider.GetChoicesAsync(option, "abilities");
    }
}

public class PerksAutocompleteProvider : IAutocompleteProvider<AutocompleteInteractionContext>
{
    public ValueTask<IEnumerable<ApplicationCommandOptionChoiceProperties>?> GetChoicesAsync(ApplicationCommandInteractionDataOption option, AutocompleteInteractionContext context)
    {
        return NWDBAutoCompleteProvider.GetChoicesAsync(option, "perks");
    }
}

public class StatusEffectsAutocompleteProvider : IAutocompleteProvider<AutocompleteInteractionContext>
{
    public ValueTask<IEnumerable<ApplicationCommandOptionChoiceProperties>?> GetChoicesAsync(ApplicationCommandInteractionDataOption option, AutocompleteInteractionContext context)
    {
        return NWDBAutoCompleteProvider.GetChoicesAsync(option, "statuseffects");
    }
}

public class QuestsAutocompleteProvider : IAutocompleteProvider<AutocompleteInteractionContext>
{
    public ValueTask<IEnumerable<ApplicationCommandOptionChoiceProperties>?> GetChoicesAsync(ApplicationCommandInteractionDataOption option, AutocompleteInteractionContext context)
    {
        return NWDBAutoCompleteProvider.GetChoicesAsync(option, "quests");
    }
}

public class CreaturesAutocompleteProvider : IAutocompleteProvider<AutocompleteInteractionContext>
{
    public ValueTask<IEnumerable<ApplicationCommandOptionChoiceProperties>?> GetChoicesAsync(ApplicationCommandInteractionDataOption option, AutocompleteInteractionContext context)
    {
        return NWDBAutoCompleteProvider.GetChoicesAsync(option, "creatures");
    }
}

public class GatherablesAutocompleteProvider : IAutocompleteProvider<AutocompleteInteractionContext>
{
    public ValueTask<IEnumerable<ApplicationCommandOptionChoiceProperties>?> GetChoicesAsync(ApplicationCommandInteractionDataOption option, AutocompleteInteractionContext context)
    {
        return NWDBAutoCompleteProvider.GetChoicesAsync(option, "gatherables");
    }
}

public class ShopsAutocompleteProvider : IAutocompleteProvider<AutocompleteInteractionContext>
{
    public ValueTask<IEnumerable<ApplicationCommandOptionChoiceProperties>?> GetChoicesAsync(ApplicationCommandInteractionDataOption option, AutocompleteInteractionContext context)
    {
        return NWDBAutoCompleteProvider.GetChoicesAsync(option, "shops");
    }
}

public class NPCSAutocompleteProvider : IAutocompleteProvider<AutocompleteInteractionContext>
{
    public ValueTask<IEnumerable<ApplicationCommandOptionChoiceProperties>?> GetChoicesAsync(ApplicationCommandInteractionDataOption option, AutocompleteInteractionContext context)
    {
        return NWDBAutoCompleteProvider.GetChoicesAsync(option, "npcs");
    }
}

public class ZonesAutoCompleteProvider : IAutocompleteProvider<AutocompleteInteractionContext>
{
    public ValueTask<IEnumerable<ApplicationCommandOptionChoiceProperties>?> GetChoicesAsync(ApplicationCommandInteractionDataOption option, AutocompleteInteractionContext context)
    {
        return NWDBAutoCompleteProvider.GetChoicesAsync(option, "zones");
    }
}

#endregion
