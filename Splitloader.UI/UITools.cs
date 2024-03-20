using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Splitloader.UI.Models;
using Splitloader.UI.ViewModels;
using Splitloader.UploadServices.Common;
using Splitloader.UploadServices.YouTube;

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
        var dir = Path.GetDirectoryName(file[0].TryGetLocalPath());
        var fileToAdd = new SelectedFile(vm)
        {
            Name = file[0].Name,
            Path = Path.Combine(dir ?? throw new InvalidOperationException(), file[0].Name)
        };
        vm.SelectedFiles.Add(fileToAdd);

        if (fileToAdd.Name[..2] == "GH")
        {
            var i = 0;
            while (true)
            {
                var nextFileNum = (int.Parse(fileToAdd.Name[2..4]) + ++i).ToString();
                if (nextFileNum.Length == 1)
                {
                    nextFileNum = $"0{nextFileNum}";
                }
                var nextFileName = $"GH{nextFileNum}{fileToAdd.Name[4..]}";
                if (!File.Exists(Path.Combine(dir, nextFileName))) break;
                var nextFileToAdd = new SelectedFile(vm)
                {
                    Name = nextFileName,
                    Path = Path.Combine(dir, nextFileName)
                };
                vm.SelectedFiles.Add(nextFileToAdd);
            }
        }
    }

    internal static async Task ConcatVideoAsync(SplitloaderViewModel vm)
    {
        var videoPartPaths = vm.SelectedFiles.Select(videoPart => videoPart.Path).ToList();
        await vm.Ffmpeg.ConcatVideoParts(videoPartPaths);
    }

    internal static async Task ConcatAndUploadVideoAsync(SplitloaderViewModel vm)
    {
        vm.DoUpload = true;
        await ConcatVideoAsync(vm);
    }

    internal static async Task UploadVideoAsync(object? sender, PropertyChangedEventArgs e, SplitloaderViewModel vm)
    {
        if (vm.Ffmpeg.ConcatStatus.Value is "failed" or null)
        {
            vm.Status = "Failed to combine videos";
            return;
        }

        if (vm.DoUpload)
        {
            await Service.UploadVideoAsync(new VideoUpload(vm.VideoTitle ?? "New Video", vm.VideoDescription ?? "",
                vm.Ffmpeg.ConcatStatus.Value));
            File.Delete(vm.Ffmpeg.ConcatStatus.Value);
            vm.DoUpload = false;
        }
        else
            vm.Status = $"Combined video at {vm.Ffmpeg.ConcatStatus.Value}";
    }
}