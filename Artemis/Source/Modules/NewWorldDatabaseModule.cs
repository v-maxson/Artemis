using LiteDB;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace Modules;

public class ItemAutocompleteProvider : IAutocompleteProvider<AutocompleteInteractionContext>
{
    private static readonly LiteDatabase DB = new("nwdb.db");

    public ValueTask<IEnumerable<ApplicationCommandOptionChoiceProperties>?> GetChoicesAsync(ApplicationCommandInteractionDataOption option, AutocompleteInteractionContext context)
    {
        var input = option.Value!;
        
        var items = DB.GetCollection<NWDB.Item>("items");
        var result = items.Find(d => d.Name.Contains(input), limit: 25).Take(25)
            .Select(d => new ApplicationCommandOptionChoiceProperties(d.Name, d.Url));

        return new(result);
    }
}

[SlashCommand("nwdb", "New World Database commands.")]
public partial class NewWorldDatabaseModule : ApplicationCommandModule<ApplicationCommandContext>
{
    #region Item Metadata
    [SubSlashCommand("item", "Search for an item in the New World Database.")]
    public partial Task ItemAsync(
        [SlashCommandParameter(Name = "name", Description = "The name of the item to search for.", AutocompleteProviderType = typeof(ItemAutocompleteProvider))]
        string item
    );
    #endregion
    public partial async Task ItemAsync(string item)
    {
        await RespondAsync(
            InteractionCallback.Message(
                new InteractionMessageProperties().WithContent($"[Here!]({item})")
            )
        );
    }
}
