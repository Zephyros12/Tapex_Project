using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Media.Imaging;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;

namespace Tapex_Project.Models;

public sealed class Detector
{
    public IReadOnlyList<DefectResult> Run(Bitmap fullBitmap, DetectorConfig cfg)
    {
        // 1) Bitmap → Mat(BGR)
        using var bgrMat = BitmapToMat(fullBitmap);

        // 2) Gray
        using var gray = new Mat();
        CvInvoke.CvtColor(bgrMat, gray, ColorConversion.Bgr2Gray);

        // 3) Adaptive Threshold (BinaryInv)
        using var bin = new Mat();
        CvInvoke.AdaptiveThreshold(
            gray, bin, 255,
            AdaptiveThresholdType.GaussianC,
            ThresholdType.BinaryInv,
            EnsureOdd(cfg.AdaptiveBlockSize),
            cfg.AdaptiveC);

        // 4) Morphology Open
        using var kernel = CvInvoke.GetStructuringElement(
            ElementShape.Rectangle,
            new System.Drawing.Size(cfg.MorphKernel, cfg.MorphKernel),
            new System.Drawing.Point(-1, -1));
        CvInvoke.MorphologyEx(bin, bin, MorphOp.Open, kernel,
                              new System.Drawing.Point(-1, -1), 1,
                              BorderType.Default, default);

        // 5) Contour 분석
        using var contours = new VectorOfVectorOfPoint();
        CvInvoke.FindContours(bin, contours, null,
                              RetrType.External, ChainApproxMethod.ChainApproxSimple);

        var results = new List<DefectResult>();

        for (int i = 0; i < contours.Size; i++)
        {
            using var cnt = contours[i];
            double area = CvInvoke.ContourArea(cnt);
            if (area < cfg.MinAreaPx || area > cfg.MaxAreaPx) continue;

            double perimeter = CvInvoke.ArcLength(cnt, true);
            double circularity = perimeter == 0 ? 0 : 4 * Math.PI * area / (perimeter * perimeter);
            if (circularity < cfg.MinCircularity) continue;

            var rect = CvInvoke.BoundingRectangle(cnt);

            results.Add(new DefectResult
            {
                X = rect.X,
                Y = rect.Y,
                Width = rect.Width,
                Height = rect.Height,
                DistanceFromEdge = Math.Min(
                    Math.Min(rect.X, bgrMat.Width - (rect.Right)),
                    Math.Min(rect.Y, bgrMat.Height - (rect.Bottom))),
                Score = circularity,
                Type = DefectType.Dust
            });
        }

        return results;
    }

    // ---------- 헬퍼 ----------

    private static int EnsureOdd(int v) => (v & 1) == 1 ? v : v + 1;

    private static Mat BitmapToMat(Bitmap bmp)
    {
        using var ms = new System.IO.MemoryStream();
        bmp.Save(ms);
        byte[] data = ms.ToArray();

        var dst = new Mat();
        CvInvoke.Imdecode(data, ImreadModes.Color, dst);

        return dst;
    }
}
