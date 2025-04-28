using System;
using System.Collections.Generic;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Tapex_Project.Models;

namespace Tapex_Project.Models.Detection
{
    public sealed class ScratchDetector : ISubDetector
    {
        public IReadOnlyList<DefectResult> Run(Mat srcGray, DetectionConfig cfg)
        {
            var p = cfg.Scratch;
            var list = new List<DefectResult>();

            // 1) Canny 엣지
            using var edges = new Mat();
            CvInvoke.Canny(srcGray, edges, p.CannyThreshold1, p.CannyThreshold2);

            // 2) 팽창으로 끊긴 엣지 연결
            using var kernel = CvInvoke.GetStructuringElement(
                ElementShape.Rectangle,
                new System.Drawing.Size(p.DilateKernel, p.DilateKernel),
                new System.Drawing.Point(-1, -1));
            using var dilated = new Mat();
            CvInvoke.MorphologyEx(edges, dilated, MorphOp.Dilate, kernel,
                                  new System.Drawing.Point(-1, -1), 1,
                                  BorderType.Default, new MCvScalar());

            // 3) 컨투어 찾기
            using var contours = new VectorOfVectorOfPoint();
            CvInvoke.FindContours(dilated, contours, null,
                                  RetrType.External, ChainApproxMethod.ChainApproxSimple);

            // 4) 필터링 & 결과 생성
            for (int i = 0; i < contours.Size; i++)
            {
                var cnt = contours[i];
                var rect = CvInvoke.BoundingRectangle(cnt);

                // 길이(mm) 계산
                double lenPx = Math.Max(rect.Width, rect.Height);
                double lenMm = lenPx * cfg.PixelSizeMicrometer / 1000.0;
                int widPx = Math.Min(rect.Width, rect.Height);

                if (lenMm < p.MinLengthMm) continue;
                if (widPx > p.MaxWidthPx) continue;

                // 점 개수(Score)와 거리 계산
                double score = cnt.Size;
                double dist = Math.Min(
                    Math.Min(rect.X, srcGray.Width - rect.Right),
                    Math.Min(rect.Y, srcGray.Height - rect.Bottom));

                list.Add(new DefectResult
                {
                    X = rect.X,
                    Y = rect.Y,
                    Width = rect.Width,
                    Height = rect.Height,
                    Score = score,
                    Type = DefectType.Scratch,
                    DistanceFromEdge = dist
                });
            }

            return list;
        }
    }
}
