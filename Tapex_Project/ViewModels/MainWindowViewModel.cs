using Tapex_Project.Services;

namespace Tapex_Project.ViewModels;

public sealed class MainWindowViewModel : ViewModelBase
{
    public ImageViewModel ImageVM { get; }
    public ResultViewModel ResultVM { get; } = new();
    public ParameterViewModel ParameterVM { get; } = new();
    public string Title => "Tapex Project";

    public MainWindowViewModel(IFilePickerService picker)
    {
        ImageVM = new ImageViewModel(picker);
    }
}
