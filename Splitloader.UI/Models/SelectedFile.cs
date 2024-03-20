using System.Threading.Tasks;
using ReactiveUI;
using Splitloader.UI.ViewModels;

namespace Splitloader.UI.Models;

public class SelectedFile(SplitloaderViewModel vm)
{
    private readonly SplitloaderViewModel _vm = vm;
    public string? Name { get; set; }
    public string? Path { get; set; }
    
    public ReactiveCommand<SelectedFile, Task> RemoveItemCommand { get; } =
        ReactiveCommand.Create<SelectedFile, Task>(RemoveItem);

    private static async Task RemoveItem(SelectedFile selectedFile)
    {
        await Task.Run(() => selectedFile._vm.SelectedFiles.Remove(selectedFile));
    }
}