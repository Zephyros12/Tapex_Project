using Tapex_Project.Models;
using Tapex_Project.Models.Detection;
using Tapex_Project.Services;
using System.ComponentModel;

namespace Tapex_Project.ViewModels;

public sealed class MainWindowViewModel : ViewModelBase
{
    public ImageViewModel ImageVM { get; }
    public ResultViewModel ResultVM { get; } = new();
    public ParameterViewModel ParameterVM { get; } = new();

    public RelayCommand DetectCmd { get; }

    public string Title => "Tapex Project";

    public MainWindowViewModel(IFilePickerService picker)
    {
        ImageVM = new ImageViewModel(picker);

        DetectCmd = new RelayCommand(_ => Detect(), _ => ImageVM.Current is not null);

        ImageVM.PropertyChanged += ImageVmOnPropertyChanged;

        ResultVM.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(ResultViewModel.SelectedResult))
                ImageVM.SelectedResult = ResultVM.SelectedResult;
        };
    }

    private void ImageVmOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ImageViewModel.Current))
            DetectCmd.NotifyCanExecuteChanged();
    }

    private void Detect()
    {
        if (ImageVM.Current is not null) return;

        var detector = new Detector();
        var results = detector.Run(
            ImageVM.Current.FullBitmap,
            ParameterVM.Config);

        ResultVM.Update(results);
    }
}
