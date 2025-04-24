namespace Tapex_Project.ViewModels;

public sealed class MainWindowViewModel : ViewModelBase
{
    public ImageViewModel ImageVM { get; } = new();
    public ResultViewModel ResultVM { get; } = new();
    public ParameterViewModel ParameterVM { get; } = new();
    public string Title => "Tapex Project";
}
