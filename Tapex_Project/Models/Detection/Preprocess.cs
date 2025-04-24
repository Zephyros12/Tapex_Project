using Emgu.CV;
using Emgu.CV.CvEnum;

namespace Tapex_Project.Models.Detection
{
    public static class Preprocess
    {
        public static Mat FlattenBackground(Mat gray8u, int radiusPx)
        {
            using var se = CvInvoke.GetStructuringElement(
                ElementShape.Ellipse,
                new System.Drawing.Size(radiusPx * 2 + 1, radiusPx * 2 + 1),
                new System.Drawing.Point(radiusPx, radiusPx));

            var background = new Mat();
            CvInvoke.MorphologyEx(gray8u, background, MorphOp.Open, se,
                                  new System.Drawing.Point(-1, -1), 1,
                                  BorderType.Default, default);

            var flat = new Mat();
            CvInvoke.Subtract(gray8u, background, flat);
            return flat;
        }
    }
}
