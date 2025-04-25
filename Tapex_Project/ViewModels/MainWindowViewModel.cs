using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Emgu.CV;
using Tapex_Project.Models;
using Tapex_Project.Models.Detection;
using Tapex_Project.Services;

namespace Tapex_Project.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public ImageViewModel ImageVM { get; }
        public ResultViewModel ResultVM { get; }
        public ParameterViewModel ParameterVM { get; }

        private readonly Detector _detector;

        private bool _isBusy;
        /// <summary>검사 중 표시용</summary>
        public bool IsBusy
        {
            get => _isBusy;
            private set
            {
                if (SetProperty(ref _isBusy, value))
                    DetectCmd.NotifyCanExecuteChanged();
            }
        }

        /// <summary>검사 명령</summary>
        public RelayCommand DetectCmd { get; }

        public MainWindowViewModel(IFilePickerService picker)
        {
            // 하위 VM 초기화
            ImageVM = new ImageViewModel(picker);
            ResultVM = new ResultViewModel();
            ParameterVM = new ParameterViewModel();

            // 서비스 + Detector 조립
            var outputService = new ProcessingOutputService();
            _detector = new Detector(
                new ISubDetector[] { new BubbleDetector() },
                outputService);

            // DetectCmd: 비동기 실행, CanExecute = !IsBusy && 이미지 로드됨
            DetectCmd = new RelayCommand(
                async _ => await DetectAsync(),
                _ => !IsBusy && ImageVM.Current != null);

            // 이미지 로드 상태 변경 시 버튼 활성화 갱신
            ImageVM.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(ImageViewModel.Current))
                    DetectCmd.NotifyCanExecuteChanged();
            };

            // 선택된 결과 변경 시 ROI 업데이트
            ResultVM.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(ResultViewModel.SelectedResult))
                {
                    RaisePropertyChanged(nameof(IsDefectSelected));
                    RaisePropertyChanged(nameof(DefectX));
                    RaisePropertyChanged(nameof(DefectY));
                    RaisePropertyChanged(nameof(DefectWidth));
                    RaisePropertyChanged(nameof(DefectHeight));
                }
            };
        }

        /// <summary>ROI 표시 여부</summary>
        public bool IsDefectSelected
            => ImageVM.Current != null
               && ResultVM.SelectedResult != null;

        public double DefectX
            => ImageVM.Current != null && ResultVM.SelectedResult != null
                ? ResultVM.SelectedResult.X * ImageVM.Current.ScaleFactor
                : 0;

        public double DefectY
            => ImageVM.Current != null && ResultVM.SelectedResult != null
                ? ResultVM.SelectedResult.Y * ImageVM.Current.ScaleFactor
                : 0;

        public double DefectWidth
            => ImageVM.Current != null && ResultVM.SelectedResult != null
                ? ResultVM.SelectedResult.Width * ImageVM.Current.ScaleFactor
                : 0;

        public double DefectHeight
            => ImageVM.Current != null && ResultVM.SelectedResult != null
                ? ResultVM.SelectedResult.Height * ImageVM.Current.ScaleFactor
                : 0;

        /// <summary>
        /// 실제 검사 실행 (백그라운드 스레드)
        /// </summary>
        private async Task DetectAsync()
        {
            if (ImageVM.Current?.FullMat == null)
                return;

            IsBusy = true;
            try
            {
                // Mat을 직접 넘겨서 검사
                var results = await Task.Run(() =>
                    _detector.Run(ImageVM.Current.FullMat, ParameterVM.Config));

                ResultVM.Update(results);
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
