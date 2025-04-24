using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;

namespace Tapex_Project.Services
{
    /// <summary>
    /// Avalonia의 OpenFileDialog를 사용하여 이미지 파일 선택 기능을 제공하는 구현체
    /// </summary>
    public sealed class FilePickerServiceAvalonia : IFilePickerService
    {
        private readonly Window _parent;

        public FilePickerServiceAvalonia(Window parent)
        {
            _parent = parent;
        }

        public async Task<List<string>> PickImageFilesAsync()
        {
            var dlg = new OpenFileDialog
            {
                Title = "이미지 파일 선택",
                AllowMultiple = true,
                Filters = new List<FileDialogFilter>
                {
                    new FileDialogFilter
                    {
                        Name = "Image files",
                        Extensions = { "png", "jpg", "jpeg", "bmp", "tif", "tiff" }
                    }
                }
            };

            var result = await dlg.ShowAsync(_parent);
            return result?.ToList() ?? new List<string>();
        }
    }
}