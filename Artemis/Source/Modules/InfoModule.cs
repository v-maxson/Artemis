using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using System.Diagnostics;
using System.Reflection;

namespace Modules;

public class InfoModule(Stopwatch stopwatch) : ApplicationCommandModule<ApplicationCommandContext>
{
    private readonly Stopwatch Stopwatch = stopwatch;

    [SlashCommand("info", "Displays information about this bot.")]
    public async Task InfoAsync() {
        await RespondAsync(InteractionCallback.DeferredMessage(MessageFlags.Ephemeral));

        var iconUrl = (await Context.Client.Rest.GetCurrentBotApplicationInformationAsync()).GetIconUrl(ImageFormat.Png)?.ToString();

        var embed = EmbedHelper.Embed(
            title: "Client Information",
            color: Colors.Pink,
            thumbnail: new EmbedThumbnailProperties(iconUrl ?? null),
            description: "This bot is open-source, click [here](https://github.com/v-maxson/Artemis) if you find a bug or have a feature request.",
            fields: [
                EmbedHelper.Field("Version:", $"v{Assembly.GetExecutingAssembly().GetName().Version}"),
                EmbedHelper.Field("Latency:", $"{Context.Client.Latency.Milliseconds}ms"),
                EmbedHelper.Field("Uptime:", Stopwatch.Elapsed.ToString(@"dd\.hh\:mm\:ss")),
                EmbedHelper.Field("Memory Usage:", $"{BytesToString(GetProcessMemoryUsage())} / {BytesToString((long?)GetInstalledMemoryCapacity())}")
            ]
        );

        await ModifyResponseAsync(msg => {
            msg.Embeds = [embed];
        });
    }

    static string BytesToString(long? byteCount) {
        if (byteCount == null) return "Unknown";

        string[] suf = ["B", "KB", "MB", "GB"];
        if (byteCount == 0)
            return "0" + suf[0];
        long bytes = Math.Abs((long)byteCount);
        int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
        double num = Math.Round(bytes / Math.Pow(1024, place), 1);
        return (Math.Sign((long)byteCount) * num).ToString() + suf[place];
    }

    private static long GetProcessMemoryUsage() {
        var process = Process.GetCurrentProcess();

        return process.PrivateMemorySize64;
    }

    private static ulong? GetInstalledMemoryCapacity() {
        var info = new Hardware.Info.HardwareInfo();

        try { info.RefreshMemoryList(); }
        catch { return null; }

        ulong totalMemory = 0;
        info.MemoryList.ForEach(info => totalMemory += info.Capacity);

        return totalMemory;
    }

}
