using System.Collections.Generic;
using Emgu.CV;
using Tapex_Project.Models;

namespace Tapex_Project.Models.Detection
{
    public interface ISubDetector
    {
        IReadOnlyList<DefectResult> Run(Mat srcGray, DetectionConfig cfg);
    }
}
