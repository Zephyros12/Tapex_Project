using Tapex_Project.Models;

namespace Tapex_Project.ViewModels;

public sealed class ParameterViewModel : ViewModelBase
{
    public DetectorConfig Config { get; } = new();
}
