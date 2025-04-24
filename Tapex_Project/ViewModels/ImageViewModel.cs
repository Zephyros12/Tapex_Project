using System.Threading.Tasks;
using System.Windows.Input;
using Tapex_Project.Models;
using Tapex_Project.Services;

namespace Tapex_Project.ViewModels
{
    /// <summary>
    /// ImageView의 ViewModel: 이미지 로드 및 Current 관리
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

        public ICommand LoadCmd { get; }

        public ImageViewModel(IFilePickerService picker)
        {
            _picker = picker;
            LoadCmd = new RelayCommand(async _ => await LoadAsync());
        }

        private async Task LoadAsync()
        {
            var files = await _picker.PickImageFilesAsync();
            if (files.Count > 0)
            {
                Current = ImageModel.FromFile(files[0]);
            }
        }
    }
}