using System.Collections.ObjectModel;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using ReactiveUI;
using Splitloader.UI.Models;

namespace Splitloader.UI.ViewModels;

public class SplitloaderViewModel : ViewModelBase
{
    public SplitloaderViewModel()
    {
        
    }

    private ObservableCollection<SelectedFile> _selectedFiles = [];
    internal ObservableCollection<SelectedFile> SelectedFiles
    {
        get => _selectedFiles;
        set => this.RaiseAndSetIfChanged(ref _selectedFiles, value);
    }
    
    private string _status = "Splitloader ready";
    public string Status
    {
        get => _status;
        set => this.RaiseAndSetIfChanged(ref _status, value);
    }

    public ReactiveCommand<SplitloaderViewModel, Task> SelectFileCommand { get; } =
        ReactiveCommand.Create<SplitloaderViewModel, Task>(SelectFile);

    private static async Task SelectFile(SplitloaderViewModel vm)
    {
        var storageProvider = new Window().StorageProvider;
        var file = await storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select Video File..."
        });
        var fileToAdd = new SelectedFile(vm)
        {
            Name = file[0].Name,
            Path = file[0].TryGetLocalPath()
        };
        vm.SelectedFiles.Add(fileToAdd);
    }
}