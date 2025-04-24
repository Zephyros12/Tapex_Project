using System.Collections.Generic;
using System.IO;

namespace Tapex_Project.Models;

public sealed class ImageLoaderService
{
    private readonly List<string> _paths = new();
    private int _index;
    public bool CanNext => _index < _paths.Count - 1;
    public bool CanPrev => _index > 0;

    public ImageModel? Load()
    {
        // TODO: OpenFileDialog → _paths 채우기
        _index = 0;
        return _paths.Count > 0 ? ImageModel.FromFile(_paths[0]) : null;
    }

    public ImageModel? Next()
    {
        if (!CanNext) return null;
        _index++;
        return ImageModel.FromFile(_paths[_index]);
    }

    public ImageModel? Prev()
    {
        if (!CanPrev) return null;
        _index--;
        return ImageModel.FromFile(_paths[_index]);
    }
}
