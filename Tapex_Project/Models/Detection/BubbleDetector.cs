using System;
using System.Collections.Generic;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Util;
using Tapex_Project.Models;

namespace Tapex_Project.Models.Detection
{
    public sealed class BubbleDetector : ISubDetector
    {
        public IReadOnlyList<DefectResult> Run(Mat srcGray, DetectionConfig cfg)
        {
            var p = cfg.Bubble;
            var results = new List<DefectResult>();

            // 1) 이진화 (Otsu)
            using var bin = new Mat();
            CvInvoke.Threshold(srcGray, bin, 0, 255,
                ThresholdType.Binary | ThresholdType.Otsu);

            // 2) 컨투어 검색
            using var contours = new VectorOfVectorOfPoint();
            CvInvoke.FindContours(bin, contours, null,
                RetrType.External, ChainApproxMethod.ChainApproxSimple);

            // 3) 파라미터(mm) -> px 변환
            double minDiaPx = UnitHelper.MmToPx(p.MinDiameterMm);
            double maxDiaPx = UnitHelper.MmToPx(p.MaxDiameterMm);

            // 4) 컨투어별 필터링
            for (int i = 0; i < contours.Size; i++)
            {
                using var cnt = contours[i];
                var rect = CvInvoke.BoundingRectangle(cnt);
                double diameter = (rect.Width + rect.Height) / 2.0;
                if (diameter < minDiaPx || diameter > maxDiaPx)
                    continue;

                double area = CvInvoke.ContourArea(cnt);
                double peri = CvInvoke.ArcLength(cnt, true);
                double circ = peri == 0
                    ? 0
                    : 4 * Math.PI * area / (peri * peri);
                if (circ < p.MinCircularity)
                    continue;

                // 5) DefectResult 생성
                results.Add(new DefectResult
                {
                    X = rect.X,
                    Y = rect.Y,
                    Width = rect.Width,
                    Height = rect.Height,
                    DistanceFromEdge = Math.Min(
                        Math.Min(rect.X, srcGray.Width - (rect.X + rect.Width)),
                        Math.Min(rect.Y, srcGray.Height - (rect.Y + rect.Height))),
                    Score = circ,
                    Type = DefectType.Bubble
                });
            }

            return results;
        }
    }
}
