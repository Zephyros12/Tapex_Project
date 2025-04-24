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
        private set 
        { 
            _current = value; 
            RaisePropertyChanged(); 
        }
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

    private DefectResult? _selected;
    public DefectResult? SelectedResult
    {
        get => _selected;
        set
        {
            _selected = value;
            UpdateOverlay();
        }
    }

    private double _ox, _oy, _ow, _oh;
    public double OverlayX { get => _ox; private set { _ox = value; RaisePropertyChanged(); } }
    public double OverlayY { get => _oy; private set { _oy = value; RaisePropertyChanged(); } }
    public double OverlayW { get => _ow; private set { _ow = value; RaisePropertyChanged(); } }
    public double OverlayH { get => _oh; private set { _oh = value; RaisePropertyChanged(); } }
    public bool OverlayVisible => _ow > 0 && _oh > 0;

    private void UpdateOverlay()
    {
        if (Current is null || SelectedResult is null)
        {
            OverlayW = OverlayH = 0;
            return;
        }

        var s = Current.ScaleFactor;
        OverlayX = SelectedResult.X * s;
        OverlayY = SelectedResult.Y * s;
        OverlayW = SelectedResult.Width * s;
        OverlayH = SelectedResult.Height * s;

        RaisePropertyChanged(nameof(OverlayVisible));
    }
}
