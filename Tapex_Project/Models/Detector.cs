using System.Collections.Generic;

namespace Tapex_Project.Models;

public sealed class Detector
{
    public IEnumerable<DefectResult> Run(ImageModel image, DetectorConfig config)
    {
        // TODO: OpenCvSharp 로직
        return new List<DefectResult>();
    }
}
