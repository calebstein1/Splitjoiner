namespace Splitloader.VideoTools;

public class FFmpegTools
{
    public static readonly ObservableString FfStatus = new();
    
    public static async Task FindOrDownloadAsync()
    {
        FfStatus.Value = $"Using {await FFmpegLinux.FindOrDownloadLinuxAsync()}";
    }
}