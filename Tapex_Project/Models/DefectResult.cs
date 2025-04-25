using Avalonia.Media.Imaging;

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
        public double X { get; init; }
        public double Y { get; init; }
        public double Width { get; init; }
        public double Height { get; init; }
        public double DistanceFromEdge { get; set; }
        public double Score { get; set; }
        public DefectType Type { get; set; }
        public double Brightness { get; set; }
        public double Sharpness { get; set; }
        public double SizeMm { get; set; }
        public Bitmap PreviewImage { get; set; } = default!;
    }
}