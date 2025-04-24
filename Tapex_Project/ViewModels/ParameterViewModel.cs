using System;
using Tapex_Project.Models.Detection;

namespace Tapex_Project.ViewModels;

public sealed class ParameterViewModel : ViewModelBase
{
    public DetectionConfig Config { get; } = new();
}
