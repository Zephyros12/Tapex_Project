using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Tapex_Project.Models;

public sealed class ImageLoaderService
{
    private readonly List<string> _paths = [];
    private int _index;

    public bool CanNext => _index < _paths.Count - 1;
    public bool CanPrev => _index > 0;

    public void SetPaths(IEnumerable<string> paths)
    {
        _paths.Clear();
        _paths.AddRange(paths.Where(File.Exists));
        _index = 0;
    }

    public ImageModel? Current() =>
        _paths.Count > 0 ? ImageModel.FromFile(_paths[_index]) : null;

    public ImageModel? Next()
    {
        if (!CanNext) return null;
        _index++;
        return Current();
    }

    public ImageModel? Prev()
    {
        if (!CanPrev) return null;
        _index--;
        return Current();
    }
}
