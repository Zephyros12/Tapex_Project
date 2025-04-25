using System;
using System.IO;
using Avalonia.Media.Imaging;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Util;
using SkiaSharp;

namespace Tapex_Project.Models
{
    /// <summary>
    /// 원본 Mat과 화면용 축소 이미지를 함께 관리하는 모델
    /// </summary>
    public sealed class ImageModel
    {
        public string FilePath { get; init; } = string.Empty;
        public Mat FullMat { get; init; } = null!;   // 원본 컬러 Mat
        public Bitmap DisplayBitmap { get; init; } = null!;   // 축소용 Avalonia.Bitmap
        public double ScaleFactor { get; init; }           // 축소 비율

        private ImageModel() { }

        /// <summary>
        /// 파일 경로로부터 Mat과 축소용 Bitmap을 생성합니다.
        /// </summary>
        /// <param name="path">이미지 파일 경로</param>
        /// <param name="maxEdge">축소본 최대 한 변 길이 (픽셀)</param>
        public static ImageModel FromFile(string path, int maxEdge = 2048)
        {
            // 1) Emgu.CV로 Mat 로드
            var mat = CvInvoke.Imread(path, ImreadModes.Color);

            // 2) Mat → PNG 바이트 벡터
            var vb = new VectorOfByte();
            CvInvoke.Imencode(".png", mat, vb);

            // 3) 바이트 배열 → MemoryStream → Avalonia.Bitmap
            using var msFull = new MemoryStream(vb.ToArray());
            var fullBmp = new Bitmap(msFull);

            // 4) 축소본 생성(SkiaSharp 사용)
            int w = fullBmp.PixelSize.Width;
            int h = fullBmp.PixelSize.Height;
            double scale = Math.Min(1.0, maxEdge / (double)Math.Max(w, h));

            Bitmap dispBmp;
            if (scale < 1.0)
            {
                using var sk = SKBitmap.Decode(path);
                using var resized = sk.Resize(
                    new SKImageInfo((int)(w * scale), (int)(h * scale)),
                    SKFilterQuality.Medium)!;
                using var img2 = SKImage.FromBitmap(resized);
                using var vb2 = img2.Encode(SKEncodedImageFormat.Png, 90);
                dispBmp = new Bitmap(vb2.AsStream());
            }
            else
            {
                dispBmp = fullBmp;
            }

            return new ImageModel
            {
                FilePath = path,
                FullMat = mat,
                DisplayBitmap = dispBmp,
                ScaleFactor = scale
            };
        }
    }
}
