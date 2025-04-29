using System.IO;
using Avalonia.Media.Imaging;
using Emgu.CV;
using Emgu.CV.Util;

namespace Tapex_Project.Models.Detection
{
    /// <summary>
    /// Mat → Avalonia Bitmap 변환용 헬퍼
    /// </summary>
    internal static class DetectorHelpers
    {
        public static Bitmap ConvertMatToBitmap(Mat mat)
        {
            // PNG 인코딩
            using var vb = new VectorOfByte();
            CvInvoke.Imencode(".png", mat, vb);
            // 메모리 스트림으로 로드
            using var ms = new MemoryStream(vb.ToArray());
            ms.Seek(0, SeekOrigin.Begin);
            return new Bitmap(ms);
        }
    }
}
