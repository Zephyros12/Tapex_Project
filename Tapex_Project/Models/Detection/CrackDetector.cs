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
    public sealed class CrackDetector : ISubDetector
    {
        private readonly IProcessingOutputService _out;

        public CrackDetector(IProcessingOutputService outputService)
        {
            _out = outputService;
        }

        public IReadOnlyList<DefectResult> Run(Mat srcGray, DetectionConfig cfg)
        {
            var p = cfg.Crack;
            var results = new List<DefectResult>();

            // 1) Canny
            using var edges = new Mat();
            CvInvoke.Canny(srcGray, edges,
                p.CannyThreshold1, p.CannyThreshold2);
            _out.SaveMat("Crack_01_Canny.png", edges);

            // 2) 스켈레톤화
            using var skel = SkeletonUtils.Thinning(edges);
            _out.SaveMat("Crack_02_Skeleton.png", skel);

            // 3) 컨투어
            using var contours = new VectorOfVectorOfPoint();
            CvInvoke.FindContours(
                skel, contours, null,
                RetrType.External,
                ChainApproxMethod.ChainApproxSimple);

            var imgSize = new Size(srcGray.Width, srcGray.Height);
            for (int i = 0; i < contours.Size; i++)
            {
                var cnt = contours[i];
                double length = CvInvoke.ArcLength(cnt, false);
                if (length < p.MinLineLengthMm * (1000.0 / cfg.PixelSizeMicrometer))
                    continue;

                var rect = CvInvoke.BoundingRectangle(cnt);
                // 테두리에 닿는 연결요소만 크랙
                bool touchesBorder =
                    rect.X <= 0 ||
                    rect.Y <= 0 ||
                    rect.Right >= imgSize.Width - 1 ||
                    rect.Bottom >= imgSize.Height - 1;
                if (!touchesBorder)
                    continue;

                results.Add(new DefectResult
                {
                    X = rect.X,
                    Y = rect.Y,
                    Width = rect.Width,
                    Height = rect.Height,
                    Type = DefectType.Crack,
                    Score = length
                });
            }

            return results;
        }
    }
}
