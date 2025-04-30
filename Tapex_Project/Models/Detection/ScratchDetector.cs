using System;
using System.Collections.Generic;
using System.Drawing;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Tapex_Project.Models;
using Tapex_Project.Services;

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

            // 1) 전역 히스토그램 평활화(EqualizeHist)로 대비 강화
            using var equalized = new Mat();
            CvInvoke.EqualizeHist(srcGray, equalized);
            _out.SaveMat("Scratch_01_EqualizeHist.png", equalized);

            // 2) TopHat / BlackHat 으로 밝은·어두운 선 강조
            var kernel = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(31, 31), new Point(-1, -1));
            using var tophat = new Mat();
            using var blackhat = new Mat();
            CvInvoke.MorphologyEx(equalized, tophat, MorphOp.Tophat, kernel, new Point(-1, -1), 1, BorderType.Default, new MCvScalar());
            CvInvoke.MorphologyEx(equalized, blackhat, MorphOp.Blackhat, kernel, new Point(-1, -1), 1, BorderType.Default, new MCvScalar());
            using var enhanced = new Mat();
            CvInvoke.Add(tophat, blackhat, enhanced);
            _out.SaveMat("Scratch_02_Enhanced.png", enhanced);

            // 3) 가우시안 블러
            using var blurred = new Mat();
            CvInvoke.GaussianBlur(enhanced, blurred, new Size(5, 5), 1.5);
            _out.SaveMat("Scratch_03_Blur.png", blurred);

            // 4) Adaptive Threshold
            using var bin = new Mat();
            CvInvoke.AdaptiveThreshold(blurred, bin, 255, AdaptiveThresholdType.GaussianC, ThresholdType.Binary, 51, -5);
            _out.SaveMat("Scratch_04_Threshold.png", bin);

            // 5) 스켈레톤화
            using var skel = SkeletonUtils.Thinning(bin);
            _out.SaveMat("Scratch_05_Skeleton.png", skel);

            // 6) 컨투어 검출
            using var contours = new VectorOfVectorOfPoint();
            CvInvoke.FindContours(skel, contours, null, RetrType.External, ChainApproxMethod.ChainApproxSimple);

            var imgSize = new Size(srcGray.Width, srcGray.Height);
            for (int i = 0; i < contours.Size; i++)
            {
                var cnt = contours[i];
                double length = CvInvoke.ArcLength(cnt, false);
                double minPix = p.MinLineLengthMm * (1000.0 / cfg.PixelSizeMicrometer);
                if (length < minPix)
                    continue;

                var rect = CvInvoke.BoundingRectangle(cnt);

                // 내부만 (테두리에 닿지 않는 것만)
                bool touchesBorder =
                    rect.X <= 0 ||
                    rect.Y <= 0 ||
                    rect.Right >= imgSize.Width - 1 ||
                    rect.Bottom >= imgSize.Height - 1;
                if (touchesBorder)
                    continue;

                // 전처리 이미지 크롭
                int pad = 10;
                int x0 = Math.Max(rect.X - pad, 0);
                int y0 = Math.Max(rect.Y - pad, 0);
                int x1 = Math.Min(rect.Right + pad, skel.Width);
                int y1 = Math.Min(rect.Bottom + pad, skel.Height);
                var roi = new Rectangle(x0, y0, x1 - x0, y1 - y0);
                using var preCrop = new Mat(skel, roi);
                var preBmp = DetectorHelpers.ConvertMatToBitmap(preCrop);

                results.Add(new DefectResult
                {
                    X = rect.X,
                    Y = rect.Y,
                    Width = rect.Width,
                    Height = rect.Height,
                    Type = DefectType.Scratch,
                    Score = length,
                    PixelSizeMicrometer = cfg.PixelSizeMicrometer,
                    PreprocessedImage = preBmp
                });
            }

            return results;
        }
    }
}