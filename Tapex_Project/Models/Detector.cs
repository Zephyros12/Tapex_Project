using System;
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

        // Models/Detector.cs
        public IReadOnlyList<DefectResult> Run(Bitmap bmp, DetectionConfig cfg)
        {
            // 1) Bitmap → 컬러 Mat
            using var srcColor = BitmapToMat(bmp);

            // 2) 컬러 → 그레이
            using var gray = new Mat();
            CvInvoke.CvtColor(srcColor, gray, ColorConversion.Bgr2Gray);

            // 3) 전체 이미지 전역 평탄화
            var flatGlobal = PreprocessingCommon.FlattenGlobal(
                gray,
                cfg.Circle.FlattenRadiusMm  // UI 상에서 설정 가능한 mm 단위 값
            );

            // 여기까지 확인: flatGlobal 영상만 복사해서 Imshow 등으로 띄워 보세요.

            // → Phase 2(반원 검출, ROI 마스킹 등) 준비
            return Array.Empty<DefectResult>();
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
