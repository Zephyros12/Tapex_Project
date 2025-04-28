using System;
using System.Collections.Generic;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Util;
using Tapex_Project.Models;

namespace Tapex_Project.Models.Detection
{
    public sealed class CrackDetector : ISubDetector
    {
        public IReadOnlyList<DefectResult> Run(Mat srcGray, DetectionConfig cfg)
        {
            var p = cfg.Crack;
            var results = new List<DefectResult>();

            // 1) Canny 엣지 검출
            using var edges = new Mat();
            CvInvoke.Canny(srcGray, edges, p.CannyThreshold1, p.CannyThreshold2);

            // 2) 허프 선 검출 (P1→P2)
            // mm 단위 파라미터를 px로 변환
            int minLenPx = (int)Math.Round(p.MinLineLengthMm * 1000.0 / cfg.PixelSizeMicrometer);
            int maxGapPx = (int)Math.Round(p.MaxLineGapMm * 1000.0 / cfg.PixelSizeMicrometer);

            var lines = CvInvoke.HoughLinesP(
                edges,
                1.0,
                Math.PI / 180.0,
                p.HoughThreshold,
                minLenPx,
                maxGapPx);

            // 3) 검출된 선분을 DefectResult로 변환
            foreach (var l in lines)
            {
                int x = Math.Min(l.P1.X, l.P2.X);
                int y = Math.Min(l.P1.Y, l.P2.Y);
                int w = Math.Abs(l.P1.X - l.P2.X);
                int h = Math.Abs(l.P1.Y - l.P2.Y);

                // 두께 보정
                int half = p.LineWidthPx / 2;
                x -= half; y -= half;
                w += p.LineWidthPx; h += p.LineWidthPx;

                // 길이(mm) 점수
                double lengthPx = Math.Sqrt(w * w + h * h);
                double lengthMm = lengthPx * cfg.PixelSizeMicrometer / 1000.0;

                // 이미지 경계로부터 거리
                double dist = Math.Min(
                    Math.Min(x, srcGray.Width - (x + w)),
                    Math.Min(y, srcGray.Height - (y + h)));

                results.Add(new DefectResult
                {
                    X = x,
                    Y = y,
                    Width = w,
                    Height = h,
                    Score = lengthMm,
                    Type = DefectType.Crack,
                    DistanceFromEdge = dist
                });
            }

            return results;
        }
    }
}
