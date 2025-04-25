// Models/Detection/BubbleDetector.cs
using System;
using System.Collections.Generic;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Tapex_Project.Models;

namespace Tapex_Project.Models.Detection
{
    public sealed class BubbleDetector : ISubDetector
    {
        public Mat? MorphKernel { get; set; }

        public IReadOnlyList<DefectResult> Run(Mat srcGray, DetectionConfig cfg)
        {
            var p = cfg.Bubble;
            var results = new List<DefectResult>();

            // 커널이 없거나 크기 변경 시 갱신
            if (MorphKernel == null || MorphKernel.Width != p.MorphKernelSize)
            {
                MorphKernel = CvInvoke.GetStructuringElement(
                    ElementShape.Rectangle,
                    new System.Drawing.Size(p.MorphKernelSize, p.MorphKernelSize),
                    new System.Drawing.Point(-1, -1));
            }

            // 1) 이진화
            using var bin = new Mat();
            CvInvoke.Threshold(srcGray, bin, p.Threshold, p.MaxValue, ThresholdType.Binary);

            // 2) 열림 연산
            CvInvoke.MorphologyEx(bin, bin, MorphOp.Open, MorphKernel,
                                  new System.Drawing.Point(-1, -1), 1,
                                  BorderType.Default, new MCvScalar());

            // 3) 외곽선 검출
            using var contours = new VectorOfVectorOfPoint();
            CvInvoke.FindContours(bin, contours, null,
                                  RetrType.External, ChainApproxMethod.ChainApproxSimple);

            for (int i = 0; i < contours.Size; i++)
            {
                var contour = contours[i];
                double area = CvInvoke.ContourArea(contour);
                if (area < p.MinArea) continue;

                var rect = CvInvoke.BoundingRectangle(contour);
                if (rect.Width < p.MinWidth || rect.Height < p.MinHeight)
                    continue;

                double peri = CvInvoke.ArcLength(contour, true);
                double circ = peri == 0
                    ? 0
                    : 4 * Math.PI * area / (peri * peri);
                if (circ < p.MinCircularity) continue;

                results.Add(new DefectResult
                {
                    X = rect.X,
                    Y = rect.Y,
                    Width = rect.Width,
                    Height = rect.Height,
                    DistanceFromEdge = Math.Min(
                        Math.Min(rect.X, srcGray.Width - rect.Right),
                        Math.Min(rect.Y, srcGray.Height - rect.Bottom)),
                    Score = circ,
                    Type = DefectType.Bubble
                });
            }

            return results;
        }
    }
}
