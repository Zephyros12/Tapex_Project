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
        private readonly IProcessingOutputService _out;

        public ScratchDetector(IProcessingOutputService outputService)
        {
            _out = outputService;
        }

        public IReadOnlyList<DefectResult> Run(Mat srcGray, DetectionConfig cfg)
        {
            var p = cfg.Scratch;
            var results = new List<DefectResult>();

            // 1) Canny
            using var edges = new Mat();
            CvInvoke.Canny(srcGray, edges,
                p.CannyThreshold1, p.CannyThreshold2);
            _out.SaveMat("Scratch_01_Canny.png", edges);

            // 2) 스켈레톤화
            using var skel = SkeletonUtils.Thinning(edges);
            _out.SaveMat("Scratch_02_Skeleton.png", skel);

            // 3) 컨투어(연결 요소) 찾기
            using var contours = new VectorOfVectorOfPoint();
            CvInvoke.FindContours(
                skel, contours, null,
                RetrType.External,
                ChainApproxMethod.ChainApproxSimple);

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

                results.Add(new DefectResult
                {
                    X = rect.X,
                    Y = rect.Y,
                    Width = rect.Width,
                    Height = rect.Height,
                    Type = DefectType.Scratch,
                    Score = length
                });
            }

            return results;
        }
    }
}
