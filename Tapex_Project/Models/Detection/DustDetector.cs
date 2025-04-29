using System.Collections.Generic;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Tapex_Project.Models;
using Tapex_Project.Services;
using System.Drawing;
using System;

namespace Tapex_Project.Models.Detection
{
    /// <summary>
    /// 먼지(Dust) 전처리 및 검출기:
    /// 동일 임계값으로 밝은(흰) 점과 어두운(검은) 점을 모두 찾습니다.
    /// </summary>
    public sealed class DustDetector : ISubDetector
    {
        private readonly IProcessingOutputService _outputService;

        public DustDetector(IProcessingOutputService outputService)
        {
            _outputService = outputService;
        }

        public IReadOnlyList<DefectResult> Run(Mat srcGray, DetectionConfig cfg)
        {
            var p = cfg.Dust;
            var results = new List<DefectResult>();

            // 1) 밝은 점 이진화 (ThresholdType.Binary)
            using var binLight = new Mat();
            CvInvoke.Threshold( srcGray, binLight, p.Threshold, 255, ThresholdType.Binary);

            // 2) 어두운 점 이진화 (ThresholdType.BinaryInv)
            using var binDark = new Mat();
            CvInvoke.Threshold( srcGray, binDark, p.Threshold, 255, ThresholdType.BinaryInv);

            // 3) 밝은/어두운 마스크 합치기
            using var bin = new Mat();
            CvInvoke.BitwiseOr(binLight, binDark, bin);

            // 4) Morphology (노이즈 제거용, 선택 사항)
            var kernel = CvInvoke.GetStructuringElement(
                ElementShape.Rectangle,
                new System.Drawing.Size(p.MorphKernel, p.MorphKernel),
                new System.Drawing.Point(-1, -1));
            CvInvoke.MorphologyEx( bin, bin, MorphOp.Open, kernel, new System.Drawing.Point(-1, -1), 1, BorderType.Default, new MCvScalar());

            // 5) 컨투어 검출 & 면적 필터링
            using var contours = new VectorOfVectorOfPoint();
            CvInvoke.FindContours(
                bin, contours, null,
                RetrType.External,
                ChainApproxMethod.ChainApproxSimple);

            for (int i = 0; i < contours.Size; i++)
            {
                var cnt = contours[i];
                double area = CvInvoke.ContourArea(cnt);
                if (area < p.MinArea || area > p.MaxArea)
                    continue;

                var rect = CvInvoke.BoundingRectangle(cnt);

                int pad = 10;
                int x0 = Math.Max(rect.X - pad, 0);
                int y0 = Math.Max(rect.Y - pad, 0);
                int x1 = Math.Min(rect.Right + pad, bin.Width);
                int y1 = Math.Min(rect.Bottom + pad, bin.Height);
                var defectArea = new Rectangle(x0, y0, x1 - x0, y1 - y0);
                using var preCrop = new Mat(bin, defectArea);
                var preBmp = DetectorHelpers.ConvertMatToBitmap(preCrop);

                results.Add(new DefectResult
                {
                    X = rect.X,
                    Y = rect.Y,
                    Width = rect.Width,
                    Height = rect.Height,
                    Type = DefectType.Dust,
                    PreprocessedImage = preBmp
                });
            }
            return results;
        }
    }
}
