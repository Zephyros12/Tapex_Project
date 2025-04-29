using System;
using System.Collections.Generic;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Tapex_Project.Models;
using Tapex_Project.Services;

namespace Tapex_Project.Models.Detection
{
    /// <summary>
    /// Hough 선 검출 기반 Scratch Detector:
    /// Border가 아닌 선분만 검출
    /// </summary>
    public sealed class ScratchDetector : ISubDetector
    {
        private readonly IProcessingOutputService _outputService;

        public ScratchDetector(IProcessingOutputService outputService)
        {
            _outputService = outputService;
        }

        public IReadOnlyList<DefectResult> Run(Mat srcGray, DetectionConfig cfg)
        {
            var p = cfg.Scratch;
            var results = new List<DefectResult>();

            // 1) Canny Edge
            using var edges = new Mat();
            CvInvoke.Canny(srcGray, edges, p.CannyThreshold1, p.CannyThreshold2);
            _outputService.SaveMat("Scratch_01_Canny.png", edges);

            // 2) HoughLinesP
            double rho = 1.0;
            double theta = Math.PI / 180.0;
            int threshold = p.HoughThreshold;
            double minLineLengthPx = p.MinLengthMm * (1000.0 / cfg.PixelSizeMicrometer);
            double maxLineGapPx = p.MaxLineGapMm  * (1000.0 / cfg.PixelSizeMicrometer);

            var lines = CvInvoke.HoughLinesP(edges, rho, theta, threshold, minLineLengthPx, maxLineGapPx);

            // 3) Border 아닌 선분만 모아서 DefectResult
            var imageSize = new System.Drawing.Size(srcGray.Width, srcGray.Height);
            foreach (var l in lines)
            {
                if (!LineDetectionUtils.IsBorderLine(l, imageSize))
                {
                    var rect = new System.Drawing.Rectangle(
                        x: (int)Math.Min(l.P1.X, l.P2.X),
                        y: (int)Math.Min(l.P1.Y, l.P2.Y),
                        width:  (int)Math.Abs(l.P1.X - l.P2.X),
                        height: (int)Math.Abs(l.P1.Y - l.P2.Y));

                    results.Add(new DefectResult
                    {
                        X = rect.X,
                        Y = rect.Y,
                        Width  = rect.Width,
                        Height = rect.Height,
                        Type   = DefectType.Scratch,
                        Score  = 1.0   // 필요 시 컨피던스 로직 추가
                    });
                }
            }

            return results;
        }
    }
}
