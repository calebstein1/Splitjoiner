using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using ReactiveUI;
using Splitloader.UI.Models;
using Splitloader.VideoTools;

namespace Splitloader.UI.ViewModels;

public class SplitloaderViewModel : ViewModelBase
{
    private readonly FFmpegTools _ffmpeg = new();
    
    internal SplitloaderViewModel()
    {
        _ffmpeg.FfStatus.PropertyChanged += (sender, e) =>
            Status = _ffmpeg.FfStatus.Value;
        _ffmpeg.ConcatStatus.PropertyChanged += (sender, e) =>
            DoUploadAsync();
        Task.Run(() => _ffmpeg.FindOrDownloadAsync());
    }

    private ObservableCollection<SelectedFile> _selectedFiles = [];
    internal ObservableCollection<SelectedFile> SelectedFiles
    {
        get => _selectedFiles;
        set => this.RaiseAndSetIfChanged(ref _selectedFiles, value);
    }
    
    private string? _status = "Splitloader ready";
    public string? Status
    {
        get => _status;
        set => this.RaiseAndSetIfChanged(ref _status, value);
    }

    public ReactiveCommand<SplitloaderViewModel, Task> SelectFileCommand { get; } =
        ReactiveCommand.Create<SplitloaderViewModel, Task>(SelectFileAsync);

    private static async Task SelectFileAsync(SplitloaderViewModel vm)
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
    
    public ReactiveCommand<SplitloaderViewModel, Task> UploadCommand { get; } =
        ReactiveCommand.Create<SplitloaderViewModel, Task>(UploadVideoAsync);

    private static async Task UploadVideoAsync(SplitloaderViewModel vm)
    {
        var videoPartPaths = vm.SelectedFiles.Select(videoPart => videoPart.Path).ToList();
        await vm._ffmpeg.ConcatVideoParts(videoPartPaths);
    }

    private void DoUploadAsync()
    {
        Status = $"Got {_ffmpeg.ConcatStatus.Value}";
    }
}