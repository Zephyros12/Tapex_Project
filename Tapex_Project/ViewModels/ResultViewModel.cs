using System.Collections.Generic;
using System.Collections.ObjectModel;
using Tapex_Project.Models;

namespace Tapex_Project.ViewModels;

public sealed class ResultViewModel : ViewModelBase
{
    private readonly Detector _detector = new();
    public ObservableCollection<DefectResult> Results { get; } = new();

    public void Update(IEnumerable<DefectResult> src)
    {
        Results.Clear();
        foreach (var r in src) Results.Add(r);
    }
}
