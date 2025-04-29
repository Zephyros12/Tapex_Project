namespace Tapex_Project.Models.Detection
{
    internal static class LineDetectionUtils
    {
        /// <summary>
        /// 선분이 영상의 가장자리에 닿아 있는지 (tolerance 픽셀 이내)
        /// </summary>
        public static bool IsBorderLine(
            Emgu.CV.Structure.LineSegment2D line,
            System.Drawing.Size imageSize,
            int tolerance = 1)
        {
            bool Touches(int coord, int limit) =>
                coord <= tolerance || coord >= limit - tolerance;

            return Touches(line.P1.X, imageSize.Width)
                || Touches(line.P1.Y, imageSize.Height)
                || Touches(line.P2.X, imageSize.Width)
                || Touches(line.P2.Y, imageSize.Height);
        }
    }
}
