
using NetCord;
using NetCord.Rest;

internal static class EmbedHelper
{
    public static EmbedProperties ErrorEmbed(string title, string description) {
        return Embed(
            title: title,
            description: description,
            color: Colors.Red
        );
    }

    public static EmbedProperties Embed(
        EmbedAuthorProperties? author = null,
        NetCord.Color? color = null,
        string? description = null,
        IEnumerable<EmbedFieldProperties>? fields = null,
        EmbedFooterProperties? footer = null,
        EmbedImageProperties? image = null,
        EmbedThumbnailProperties? thumbnail = null,
        DateTimeOffset? timestamp = null,
        string? title = null,
        string? url = null
        ) {
        return new EmbedProperties()
            .WithAuthor(author)
            .WithColor(color ?? default)
            .WithDescription(description)
            .WithFields(fields)
            .WithFooter(footer)
            .WithImage(image)
            .WithThumbnail(thumbnail)
            .WithTimestamp(timestamp)
            .WithTitle(title)
            .WithUrl(url);
    }

    public static EmbedAuthorProperties Author(string name, string? iconUrl = null, string? url = null) {
        return new EmbedAuthorProperties()
            .WithName(name)
            .WithIconUrl(iconUrl)
            .WithUrl(url);
    }

    public static EmbedFooterProperties Footer(string? text, string? iconUrl = null) {
        return new EmbedFooterProperties()
            .WithText(text)
            .WithIconUrl(iconUrl);
    }

    public static EmbedFieldProperties Field(string? name, string? value, bool inline = false) {
        return new EmbedFieldProperties()
            .WithName(name)
            .WithValue(value)
            .WithInline(inline);
    }

    public static EmbedThumbnailProperties Thumbnail(string? url) {
        return new EmbedThumbnailProperties(url);
    }

    public static EmbedImageProperties Image(string? url) {
        return new EmbedImageProperties(url);
    }
}
