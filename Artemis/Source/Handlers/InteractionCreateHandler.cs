using NetCord.Gateway;
using NetCord.Hosting.Gateway;
using NetCord;
using Serilog;

namespace Handlers;

[GatewayEvent(nameof(GatewayClient.InteractionCreate))]
public class InteractionCreateHandler : IGatewayEventHandler<Interaction>
{
    public ValueTask HandleAsync(Interaction interaction) {
        if (interaction is ApplicationCommandInteraction appCommand) {
            Log.Information($"Received ApplicationCommandInteraction from {appCommand!.User.GlobalName} ({appCommand.User.Id}) -> {appCommand.Data.Name}");
            return default;
        }
        else if (interaction is ComponentInteraction component) {
            Log.Information($"Received ComponentInteraction from {component!.User.GlobalName} ({component.User.Id}) -> {component.Data.CustomId}");
            return default;
        }
        else if (interaction is AutocompleteInteraction autocomplete) {
            Log.Information($"Received AutocompleteInteraction from {autocomplete!.User.GlobalName} ({autocomplete.User.Id}) -> {autocomplete.Data.Name}");
            return default;
        }
        else {
            Log.Information($"Received Interaction from {interaction.User.GlobalName} ({interaction.User.Id}) -> Unknown");
            return default;
        }
    }
}
