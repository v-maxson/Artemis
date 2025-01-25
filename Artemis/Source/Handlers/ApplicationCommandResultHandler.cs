using System.Net;
using Microsoft.Extensions.Logging;
using NetCord;
using NetCord.Gateway;
using NetCord.Hosting.Services.ApplicationCommands;
using NetCord.Rest;
using NetCord.Services;
using NetCord.Services.ApplicationCommands;

namespace Handlers;

public class ApplicationCommandResultHandler<TContext>(MessageFlags? messageFlags = null)
    : IApplicationCommandResultHandler<TContext>
    where TContext : IApplicationCommandContext
{
    public async ValueTask HandleResultAsync(IExecutionResult result, TContext context, GatewayClient? client, ILogger logger, IServiceProvider services) {
        if (result is not IFailResult failResult) return;

        var resultMessage = failResult.Message;
        var interaction = context.Interaction;

        // Log exceptions.
        if (failResult is IExceptionResult exceptionResult)
            logger.LogError(exceptionResult.Exception, "Execution of an application command of name '{Name}' failed with an exception", interaction.Data.Name);
        else
            logger.LogDebug("Execution of an application command of name '{Name}' failed with '{Message}'", interaction.Data.Name, resultMessage);

        var message = new InteractionMessageProperties()
            .WithContent(null)
            .WithEmbeds([
                EmbedHelper.ErrorEmbed("Unhandled Command Error",
                $"An error occurred while executing the command: {resultMessage}\n\nPlease message an administrator if this issue persists.")
            ])
            .WithFlags(messageFlags ?? MessageFlags.Ephemeral);

        // Check if the interaction has been responded to.

        try {
            await interaction.SendResponseAsync(InteractionCallback.Message(message));
        }
        catch (RestException restException)
        when (restException.StatusCode is HttpStatusCode.BadRequest && restException.Error is { Code: 40060 }) {
            await interaction.SendFollowupMessageAsync(message);
        }
    }
}
