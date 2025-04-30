using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.CvEnum;
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
        public IPreprocessingService Preprocessor { get; }

        private Mat? _storedMask;
        public Mat? StoredMask
        {
            get => _storedMask;
            private set
            {
                if (SetProperty(ref _storedMask, value))
                {
                    GenerateMaskCmd.NotifyCanExecuteChanged();
                }
            }
        }

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

        /// <summary>Mask 생성</summary>
        public RelayCommand GenerateMaskCmd { get; }

        /// <summary>Mask 초기화</summary>
        public RelayCommand ClearMaskCmd { get; }

        /// <summary>검사 명령</summary>
        public RelayCommand DetectCmd { get; }

        private readonly Detector _detector;

        private bool _isParameterPanelExpanded = true;
        public bool IsParameterPanelExpanded
        {
            get => _isParameterPanelExpanded;
            set => SetProperty(ref _isParameterPanelExpanded, value);
        }

        
        private string? _statusMessage;
        public string? StatusMessage
        {
            get => _statusMessage;
            private set => SetProperty(ref _statusMessage, value);
        }

        private bool _showAllDefects;
        public bool ShowAllDefects
        {
            get => _showAllDefects;
            set
            {
                if (SetProperty(ref _showAllDefects, value))
                {
                    // 체크박스 토글 시에도 AllResults 바인딩 갱신
                    RaisePropertyChanged(nameof(AllResults));
                }
            }
        }

        public IEnumerable<DefectResult> AllResults => ResultVM.Results;

        public MainWindowViewModel(IFilePickerService picker)
        {
            // 하위 VM 초기화
            ImageVM = new ImageViewModel(picker);
            ResultVM = new ResultViewModel();
            ParameterVM = new ParameterViewModel();

            ResultVM.Results.CollectionChanged += (s, e) =>
            {
                RaisePropertyChanged(nameof(AllResults));
            };

            Preprocessor = new PreprocessingService();
            GenerateMaskCmd = new RelayCommand(_ => GenerateMask(), _ => ImageVM.Current?.FullMat != null && StoredMask == null);
            ClearMaskCmd = new RelayCommand(_ =>
            {
                StoredMask = null;
                StatusMessage = "Mask 초기화 완료";
                GenerateMaskCmd.NotifyCanExecuteChanged();
            });

            // 서비스 + Detector 조립
            _detector = new Detector(new ISubDetector[]
            {
                new BubbleDetector(),
                new DustDetector(),
                new ScratchDetector(),
                new CrackDetector(),
            }, Preprocessor);

            // DetectCmd: 비동기 실행, CanExecute = !IsBusy && 이미지 로드됨
            DetectCmd = new RelayCommand(
                async _ => await DetectAsync(),
                _ => !IsBusy && ImageVM.Current?.FullMat != null);

            // 이미지 로드 상태 변경 시 버튼 활성화 갱신
            ImageVM.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(ImageViewModel.Current))
                {
                    GenerateMaskCmd.NotifyCanExecuteChanged();
                    DetectCmd.NotifyCanExecuteChanged();
                }
            };

            ShowAllDefects = false;

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

        private void GenerateMask()
        {
            var mat = ImageVM.Current?.FullMat;
            if (mat == null) return;

            var gray = new Mat();
            CvInvoke.CvtColor(mat, gray, ColorConversion.Bgr2Gray);
            StoredMask = Preprocessor.ExtractLargestBlobMask(gray);

            StatusMessage = "Mask 생성 완료";

            GenerateMaskCmd.NotifyCanExecuteChanged();
        }

        /// <summary>
        /// 실제 검사 실행 (백그라운드 스레드)
        /// </summary>
        private async Task DetectAsync()
        {
            var mat = ImageVM.Current?.FullMat;
            if (mat == null) return;

            IsBusy = true;
            try
            {
                Mat roi;
                if (StoredMask != null)
                {
                    roi = new Mat();
                    CvInvoke.BitwiseAnd(mat, mat, roi, StoredMask);
                }
                else
                {
                    roi = mat;
                    StatusMessage = "Mask 없이 전체 영상 검사";
                }

                var results = await Task.Run(() => _detector.Run(roi, ParameterVM.Config));

                ResultVM.Update(results);
            }
            finally
            {
                IsBusy = false;
            }
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
    }
}
