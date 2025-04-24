using System.Collections.Generic;
using Avalonia.Media.Imaging;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Tapex_Project.Models.Detection;

namespace Tapex_Project.Models
{
    public sealed class Detector
    {
        private readonly ISubDetector[] _subDetectors =
        {
            new BubbleDetector()
            // TODO: ScratchDetector, DustDetector, CrackDetector 추가
        };

        public IReadOnlyList<DefectResult> Run(Bitmap bmp, DetectionConfig cfg)
        {
            // 1) Bitmap → Mat (컬러)
            using var srcColor = BitmapToMat(bmp);

            // 2) 컬러 → 그레이
            using var gray = new Mat();
            CvInvoke.CvtColor(srcColor, gray, ColorConversion.Bgr2Gray);

            // 3) 각 서브-검출기에 평탄화된 gray Mat 전달
            var results = new List<DefectResult>();
            foreach (var det in _subDetectors)
            {
                results.AddRange(det.Run(gray, cfg));
            }

            return results;
        }

        // Bitmap → Mat 변환 헬퍼
        private static Mat BitmapToMat(Bitmap bmp)
        {
            using var ms = new System.IO.MemoryStream();
            bmp.Save(ms);
            var data = ms.ToArray();
            var dst = new Mat();
            CvInvoke.Imdecode(data, ImreadModes.Color, dst);
            return dst;
        }
    }
}
