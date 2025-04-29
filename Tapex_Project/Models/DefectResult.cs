using Avalonia.Media.Imaging;
using System.Collections.Generic;

namespace Tapex_Project.Models
{
    public enum DefectType
    {
        Unknown,
        Bubble,
        Scratch,
        Dust,
        Crack
    }

    public sealed class DefectResult
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public double PixelSizeMicrometer { get; set; }
        public double WidthMm => Width * (PixelSizeMicrometer / 1000.0);
        public double HeightMm => Height * (PixelSizeMicrometer / 1000.0);
        public double DistanceFromEdge { get; set; }
        public double Score { get; set; }
        public DefectType Type { get; set; }
        public double Brightness { get; set; }
        public double Sharpness { get; set; }
        public double SizeMm { get; set; }
        public Bitmap PreviewImage { get; set; } = default!;
        public Bitmap? PreprocessedImage { get; set; }
    }
}