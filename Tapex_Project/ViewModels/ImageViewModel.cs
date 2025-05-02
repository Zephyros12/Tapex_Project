using System.Threading.Tasks;
using System.Windows.Input;
using Tapex_Project.Models;
using Tapex_Project.Services;
using Tapex_Project.Views;
using Av = Avalonia.Media.Imaging;
using SD = System.Drawing;
using System.IO;
using Emgu.CV;
using Emgu.CV.Util;

namespace Tapex_Project.ViewModels
{
    /// <summary>
    /// View의 ViewModel: 이미지 로드 및 Current 관리
    /// </summary>
    public class ImageViewModel : ViewModelBase
    {
        private readonly IFilePickerService _picker;

        private ImageModel? _current;
        public ImageModel? Current
        {
            get => _current;
            private set => SetProperty(ref _current, value);
        }

        private Av.Bitmap? _maskBitmap;
        public Av.Bitmap? MaskBitmap
        {
            get => _maskBitmap;
            private set
            {
                if (SetProperty(ref _maskBitmap, value))
                    (ViewMaskCmd as RelayCommand)?.NotifyCanExecuteChanged();
            }
        }

        public ICommand LoadCmd { get; }
        public ICommand ViewMaskCmd { get; }

        public ImageViewModel(IFilePickerService picker)
        {
            _picker = picker;
            LoadCmd = new RelayCommand(async _ => await LoadAsync());
            ViewMaskCmd = new RelayCommand(_ => ShowMaskWindow(), _ => MaskBitmap != null);
        }

        private async Task LoadAsync()
        {
            var files = await _picker.PickImageFilesAsync();
            if (files.Count > 0)
            {
                Current = ImageModel.FromFile(files[0]);
            }
        }

        public void SetMask(Mat mat)
        {
            using var buf = new VectorOfByte();
            CvInvoke.Imencode(".png", mat, buf);

            using var ms = new MemoryStream(buf.ToArray());
            MaskBitmap = new Av.Bitmap(ms);
        }

        public void ClearMask() => MaskBitmap = null;

        private void ShowMaskWindow()
        {
            if (MaskBitmap == null) return;

            var win = new MaskPreviewWindow
            {
                DataContext = this
            };
            win.Show();
        }
    }
}