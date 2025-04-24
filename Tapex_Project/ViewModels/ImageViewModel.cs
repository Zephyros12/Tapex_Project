using System.Threading.Tasks;
using Tapex_Project.Models;
using Tapex_Project.Services;

namespace Tapex_Project.ViewModels;

public sealed class ImageViewModel : ViewModelBase
{
    private readonly ImageLoaderService _ldr = new();
    private ImageModel? _current;

    public ImageModel? Current
    {
        get => _current;
        private set { _current = value; RaisePropertyChanged(); }
    }

    public RelayCommand LoadCmd { get; }
    public RelayCommand NextCmd { get; }
    public RelayCommand PrevCmd { get; }

    private readonly IFilePickerService _picker;

    public ImageViewModel(IFilePickerService picker)
    {
        _picker = picker;

        LoadCmd = new RelayCommand(async _ => await LoadAsync());
        NextCmd = new RelayCommand(_ => Current = _ldr.Next(), _ => _ldr.CanNext);
        PrevCmd = new RelayCommand(_ => Current = _ldr.Prev(), _ => _ldr.CanPrev);
    }

    private async Task LoadAsync()
    {
        var files = await _picker.PickImageFilesAsync();
        if (files.Count == 0) return;

        _ldr.SetPaths(files);
        Current = _ldr.Current();
        NextCmd.NotifyCanExecuteChanged();
        PrevCmd.NotifyCanExecuteChanged();
    }
}
