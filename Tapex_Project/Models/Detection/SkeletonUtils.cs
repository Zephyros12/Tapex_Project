// Models/Detection/SkeletonUtils.cs
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System.Drawing;

namespace Tapex_Project.Models.Detection
{
    internal static class SkeletonUtils
    {
        /// <summary>
        /// src 바이너리 이미지를 스켈레톤(1px 두께)으로 변환
        /// </summary>
        public static Mat Thinning(Mat src)
        {
            var skel = new Mat(src.Size, DepthType.Cv8U, 1);
            skel.SetTo(new MCvScalar(0));
            using var temp = new Mat(src.Size, DepthType.Cv8U, 1);
            using var eroded = new Mat(src.Size, DepthType.Cv8U, 1);
            var element = CvInvoke.GetStructuringElement(
                ElementShape.Cross, new Size(3, 3), new Point(-1, -1));

            var bin = src.Clone(); // 작업 복사본
            while (CvInvoke.CountNonZero(bin) > 0)
            {
                // 침식 → 팽창 → 차집합 → 스켈에 추가
                CvInvoke.MorphologyEx(bin, eroded, MorphOp.Erode, element,
                                      new Point(-1, -1), 1, BorderType.Constant, new MCvScalar(0));
                CvInvoke.MorphologyEx(eroded, temp, MorphOp.Dilate, element,
                                      new Point(-1, -1), 1, BorderType.Constant, new MCvScalar(0));
                CvInvoke.Subtract(bin, temp, temp);
                CvInvoke.BitwiseOr(skel, temp, skel);
                eroded.CopyTo(bin);
            }
            return skel;
        }
    }
}
