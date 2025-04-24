using Tapex_Project.Models;
using Tapex_Project.Models.Detection;
using Tapex_Project.Services;

namespace Tapex_Project.ViewModels
{
    /// <summary>
    /// MainWindow의 ViewModel: 하위 VM과 Detect 명령 연결
    /// </summary>
    public class MainWindowViewModel : ViewModelBase
    {
        public ImageViewModel ImageVM { get; }
        public ResultViewModel ResultVM { get; } = new();
        public ParameterViewModel ParameterVM { get; } = new();
        public RelayCommand DetectCmd { get; }

        public MainWindowViewModel(IFilePickerService picker)
        {
            ImageVM = new ImageViewModel(picker);

            DetectCmd = new RelayCommand(_ => Detect(), _ => ImageVM.Current != null);
            ImageVM.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(ImageViewModel.Current))
                    DetectCmd.NotifyCanExecuteChanged();
            };
        }

        private void Detect()
        {
            if (ImageVM.Current == null) return;

            var detector = new Detector();
            var results = detector.Run(ImageVM.Current.FullBitmap, ParameterVM.Config);
            ResultVM.Update(results);
        }
    }
}
