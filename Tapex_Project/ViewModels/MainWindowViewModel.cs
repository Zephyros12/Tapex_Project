using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Emgu.CV;
using Tapex_Project.Models;
using Tapex_Project.Models.Detection;
using Tapex_Project.Services;
using Tapex_Project.ViewModels;

namespace Tapex_Project.ViewModels
{
    /// <summary>
    /// MainWindow의 ViewModel: 이미지 로드, 검사 실행 및 결과 표시를 MVVM 구조로 최적화
    /// </summary>
    public class MainWindowViewModel : ViewModelBase
    {
        public ImageViewModel ImageVM { get; }
        public ResultViewModel ResultVM { get; }
        public ParameterViewModel ParameterVM { get; }

        private readonly Detector _detector;

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            private set => SetProperty(ref _isBusy, value);
        }

        public RelayCommand DetectCmd { get; }

        public MainWindowViewModel(IFilePickerService picker)
        {
            ImageVM = new ImageViewModel(picker);
            ResultVM = new ResultViewModel();
            ParameterVM = new ParameterViewModel();

            var outputService = new ProcessingOutputService();
            _detector = new Detector(
                new ISubDetector[]
                {
                    new BubbleDetector(),
                    // 필요한 경우 다른 디텍터 주입
                },
                outputService);

            DetectCmd = new RelayCommand(
                async _ => await DetectAsync(),
                _ => !IsBusy && ImageVM.Current != null);

            ImageVM.PropertyChanged += OnImageVMPropertyChanged;
        }

        private void OnImageVMPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ImageViewModel.Current))
                DetectCmd.NotifyCanExecuteChanged();
        }

        private async Task DetectAsync()
        {
            if (ImageVM.Current?.FullMat is null)
                return;

            IsBusy = true;
            DetectCmd.NotifyCanExecuteChanged();

            try
            {
                // Mat을 직접 전달하여 변환 비용 제거
                var results = await Task.Run(() =>
                    _detector.Run(ImageVM.Current.FullMat, ParameterVM.Config));

                ResultVM.Update(results);
            }
            finally
            {
                IsBusy = false;
                DetectCmd.NotifyCanExecuteChanged();
            }
        }
    }
}
