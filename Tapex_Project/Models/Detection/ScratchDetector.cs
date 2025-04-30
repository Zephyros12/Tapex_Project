using System;
using System.Collections.Generic;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Util;
using Tapex_Project.Models;
using Tapex_Project.Services;
using Emgu.CV.Structure;
using System.Drawing;

namespace Tapex_Project.Models.Detection
{
    public sealed class ScratchDetector : ISubDetector
    {
        public IReadOnlyList<DefectResult> Run(Mat srcGray, DetectionConfig cfg)
        {
            var p = cfg.Scratch;
            var results = new List<DefectResult>();

            // 1) Canny
            using var edges = new Mat();
            CvInvoke.Canny(srcGray, edges, p.CannyThreshold1, p.CannyThreshold2);

            // 2) 스켈레톤화
            using var skel = SkeletonUtils.Thinning(edges);

            // 3) 컨투어(연결 요소) 찾기
            using var contours = new VectorOfVectorOfPoint();
            CvInvoke.FindContours(skel, contours, null, RetrType.External, ChainApproxMethod.ChainApproxSimple);

            for (int i = 0; i < contours.Size; i++)
            {
                var cnt = contours[i];
                double length = CvInvoke.ArcLength(cnt, false);
                if (length < p.MinLineLengthMm * (1000.0 / cfg.PixelSizeMicrometer))
                    continue;

                var rect = CvInvoke.BoundingRectangle(cnt);
                // 내부만 → 테두리 닿지 않는 것만
                var imgSize = new Size(srcGray.Width, srcGray.Height);
                bool touchesBorder =
                    rect.X <= 0 ||
                    rect.Y <= 0 ||
                    rect.Right >= imgSize.Width - 1 ||
                    rect.Bottom >= imgSize.Height - 1;
                if (touchesBorder)
                    continue;

                int pad = 10;
                int x0 = Math.Max(rect.X - pad, 0);
                int y0 = Math.Max(rect.Y - pad, 0);
                int x1 = Math.Min(rect.Right + pad, skel.Width);
                int y1 = Math.Min(rect.Bottom + pad, skel.Height);
                var defectArea = new Rectangle(x0, y0, x1 - x0, y1 - y0);
                using var preCrop = new Mat(skel, defectArea);
                var preBmp = DetectorHelpers.ConvertMatToBitmap(preCrop);

                results.Add(new DefectResult
                {
                    X = rect.X,
                    Y = rect.Y,
                    Width = rect.Width,
                    Height = rect.Height,
                    Type = DefectType.Scratch,
                    Score = length,
                    PreprocessedImage = preBmp
                });
            }

            return results;
        }
    }
}