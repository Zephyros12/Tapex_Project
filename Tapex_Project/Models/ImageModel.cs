using System;
using Avalonia.Media.Imaging;
using SkiaSharp;
using System.IO;

namespace Tapex_Project.Models;

public sealed class ImageModel
{
    public string FilePath { get; init; } = string.Empty;
    public Bitmap FullBitmap { get; init; } = null!;
    public Bitmap DisplayBitmap { get; init; } = null!;
    public double ScaleFactor { get; init; }

    public static ImageModel FromFile(string path, int maxEdge = 2048)
    {
        using var fs = File.OpenRead(path);
        var full = new Bitmap(fs);

        var w = full.PixelSize.Width;
        var h = full.PixelSize.Height;
        var scale = Math.Min(1.0, maxEdge / (double)Math.Max(w, h));

        Bitmap display;
        if (scale < 1.0)
        {
            var targetW = (int)(w* scale);
            var targetH = (int)(h* scale);

            using var skBitmap = SKBitmap.Decode(path);
            using var resized = skBitmap.Resize(new SKImageInfo(targetW, targetH), SKFilterQuality.Medium)!;

            using var image = SKImage.FromBitmap(resized);
            using var data = image.Encode(SKEncodedImageFormat.Png, 90);
            display = new Bitmap(data.AsStream());
        }
        else
        {
            display = full;
        }

        return new ImageModel
        {
            FilePath = path,
            FullBitmap = full,
            DisplayBitmap = display,
            ScaleFactor = scale
        };
    }
}
