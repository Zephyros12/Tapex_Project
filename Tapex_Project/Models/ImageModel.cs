using System;
using System.IO;
using Avalonia.Media.Imaging;
using SkiaSharp;

namespace Tapex_Project.Models
{
    /// <summary>
    /// 원본 이미지와 화면용 축소 이미지를 함께 관리하는 모델
    /// </summary>
    public sealed class ImageModel
    {
        public string FilePath { get; init; } = string.Empty;
        public Bitmap FullBitmap { get; init; } = null!;   // 원본
        public Bitmap DisplayBitmap { get; init; } = null!; // 축소본
        public double ScaleFactor { get; init; }           // 축소 비율 (Display / Full)

        /// <summary>
        /// 파일 경로로부터 원본 및 축소 이미지를 생성합니다.
        /// </summary>
        /// <param name="maxEdge">축소본 최대 한 변 길이 (픽셀)</param>
        public static ImageModel FromFile(string path, int maxEdge = 2048)
        {
            // 원본 비트맵 로드
            using var fs = File.OpenRead(path);
            var full = new Bitmap(fs);

            // 원본 크기
            var w = full.PixelSize.Width;
            var h = full.PixelSize.Height;
            var scale = Math.Min(1.0, maxEdge / (double)Math.Max(w, h));

            Bitmap disp;
            if (scale < 1.0)
            {
                int targetW = (int)(w * scale);
                int targetH = (int)(h * scale);

                using var sk = SKBitmap.Decode(path);
                using var resized = sk.Resize(new SKImageInfo(targetW, targetH), SKFilterQuality.Medium)!;
                using var img = SKImage.FromBitmap(resized);
                using var encoded = img.Encode(SKEncodedImageFormat.Png, 90);
                disp = new Bitmap(encoded.AsStream());
            }
            else
            {
                disp = full;
            }

            return new ImageModel
            {
                FilePath = path,
                FullBitmap = full,
                DisplayBitmap = disp,
                ScaleFactor = scale
            };
        }
    }
}
