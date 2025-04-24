using System;
using Avalonia.Controls;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Tapex_Project.Services;

public sealed class FilePickerServiceAvalonia : IFilePickerService
{
    private readonly Window _owner;

    public FilePickerServiceAvalonia(Window owner) => _owner = owner;

    public async Task<IReadOnlyList<string>> PickImageFilesAsync()
    {
        var dlg = new OpenFileDialog
        {
            Title = "Select image files",
            AllowMultiple = true,
            Filters =
            {
                new FileDialogFilter { Name = "Image", Extensions = { "bmp", "png", "jpg", "tif" } },
                new FileDialogFilter { Name = "All",   Extensions = { "*" } }
            }
        };
        return await dlg.ShowAsync(_owner) ?? Array.Empty<string>();
    }
}
