using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Splitloader.UI.Models;
using Splitloader.UI.ViewModels;

namespace Splitloader.UI;

internal static class UiTools
{
    internal static async Task SelectFileAsync(SplitloaderViewModel vm)
    {
        var storageProvider = new Window().StorageProvider;
        var file = await storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select Video File...",
            FileTypeFilter = new[] { VideoFileTypes.Types }
        });
        var fileToAdd = new SelectedFile(vm)
        {
            Name = file[0].Name,
            Path = file[0].TryGetLocalPath()
        };
        vm.SelectedFiles.Add(fileToAdd);
    }

    internal static async Task ConcatVideoAsync(SplitloaderViewModel vm)
    {
        var videoPartPaths = vm.SelectedFiles.Select(videoPart => videoPart.Path).ToList();
        await vm.Ffmpeg.ConcatVideoParts(videoPartPaths);
    }

    internal static async Task UploadVideoAsync(object? sender, PropertyChangedEventArgs e, SplitloaderViewModel vm)
    {
        await Task.Run(() => vm.Status = $"Got {vm.Ffmpeg.ConcatStatus.Value}");
    }
}