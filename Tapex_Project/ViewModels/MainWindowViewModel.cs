using Tapex_Project.Models;
using Tapex_Project.Services;

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
        ImageVM.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(ImageViewModel.Current))
                DetectCmd.NotifyCanExecuteChanged();
        };
    }

    private void Detect()
    {
        if (ImageVM.Current is not null) return;

        var detector = new Detector();
        var results = detector.Run(ImageVM.Current.FullBitmap, ParameterVM.Config);

        ResultVM.Update(results);
    }
}
