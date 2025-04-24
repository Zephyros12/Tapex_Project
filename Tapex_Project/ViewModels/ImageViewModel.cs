using Tapex_Project.Models;

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

    public ImageViewModel()
    {
        LoadCmd = new RelayCommand(_ => Current = _ldr.Load());
        NextCmd = new RelayCommand(_ => Current = _ldr.Next(), _ => _ldr.CanNext);
        PrevCmd = new RelayCommand(_ => Current = _ldr.Prev(), _ => _ldr.CanPrev);
    }
}
