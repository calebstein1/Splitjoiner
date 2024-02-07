using Avalonia.Platform.Storage;

namespace Splitloader.UI.Models;

internal static class VideoFileTypes
{
    internal static FilePickerFileType Types { get; } = new("All Videos")
    {
        Patterns = new[] { "*.mp4", "*.mov", "*.mkv", "*.webm" },
        MimeTypes = new[] { "video/*" }
    };
}