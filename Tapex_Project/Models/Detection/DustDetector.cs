using System;
using System.Collections.Generic;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Tapex_Project.Models;

namespace Tapex_Project.Models.Detection
{
    /// <summary>
    /// Dust(작은 먼지점) 검출기
    /// </summary>
    public sealed class DustDetector : ISubDetector
    {
        private Mat _kernel;

        public IReadOnlyList<DefectResult> Run(Mat srcGray, DetectionConfig cfg)
        {
            var p = cfg.Dust;

            // 커널이 없거나 크기 변경 시 갱신
            if (_kernel == null || _kernel.Width != p.MorphKernel)
            {
                _kernel = CvInvoke.GetStructuringElement(
                    ElementShape.Ellipse,
                    new System.Drawing.Size(p.MorphKernel, p.MorphKernel),
                    new System.Drawing.Point(-1, -1));
            }

            var results = new List<DefectResult>();

            // 1) 밝은 배경에 어두운点 검출 → inverse binary
            using var bin = new Mat();
            CvInvoke.Threshold(
                srcGray, bin,
                p.Threshold,        // 임계값
                255,                // maxValue
                ThresholdType.BinaryInv);

            // 2) 노이즈 제거 위해 열림 연산
            CvInvoke.MorphologyEx(
                bin, bin,
                MorphOp.Open,
                _kernel,
                new System.Drawing.Point(-1, -1),
                1,
                BorderType.Default,
                new MCvScalar());

            // 3) 외곽선 검출
            using var contours = new VectorOfVectorOfPoint();
            CvInvoke.FindContours(
                bin, contours, null,
                RetrType.External,
                ChainApproxMethod.ChainApproxSimple);

            // 4) 컨투어 별 필터링 후 결과
            for (int i = 0; i < contours.Size; i++)
            {
                var c = contours[i];
                double area = CvInvoke.ContourArea(c);
                if (area < p.MinArea || area > p.MaxArea)
                    continue;

                var rect = CvInvoke.BoundingRectangle(c);
                results.Add(new DefectResult
                {
                    X = rect.X,
                    Y = rect.Y,
                    Width = rect.Width,
                    Height = rect.Height,
                    DistanceFromEdge = Math.Min(
                        Math.Min(rect.X, srcGray.Width - rect.Right),
                        Math.Min(rect.Y, srcGray.Height - rect.Bottom)),
                    Score = area,
                    Type = DefectType.Dust
                });
            }

            return results;
        }
    }
}
