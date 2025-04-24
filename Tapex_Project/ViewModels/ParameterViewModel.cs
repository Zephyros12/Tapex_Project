using System;
using Tapex_Project.Models;

namespace Tapex_Project.ViewModels;

public sealed class ParameterViewModel : ViewModelBase
{
    public DetectorConfig Config { get; } = new();

    private static readonly double _px2ToMm2 = UnitHelper.PixelPitchMm * UnitHelper.PixelPitchMm;

    public double MinAreaMm2
    {
        get => Config.MinAreaPx * _px2ToMm2;
        set
        {
            Config.MinAreaPx = (int)Math.Round(value / _px2ToMm2);
            RaisePropertyChanged();
        }
    }

    public double MaxAteaMm2
    {
        get => Config.MaxAreaPx * _px2ToMm2;
        set
        {
            Config.MaxAreaPx = (int)Math.Round(value * _px2ToMm2);
            RaisePropertyChanged();
        }
    }
}
