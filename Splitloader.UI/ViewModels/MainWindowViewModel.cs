namespace Splitloader.UI.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public MainWindowViewModel()
    {
        Splitloader = new SplitloaderViewModel();
    }
    
    public SplitloaderViewModel Splitloader { get; }
}