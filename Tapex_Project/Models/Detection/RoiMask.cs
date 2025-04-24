using System.Drawing;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

namespace Tapex_Project.Models.Detection
{
    /// <summary>
    /// 검출된 CircleF를 바탕으로 하단 반원 ROI 마스크를 생성합니다.
    /// </summary>
    public static class RoiMask
    {
        /// <summary>
        /// width×height 크기의 단일 채널 Mat에,
        /// 주어진 원의 하단 반원 영역만 255, 나머지 0으로 설정합니다.
        /// </summary>
        /// <param name="width">이미지 폭(px)</param>
        /// <param name="height">이미지 높이(px)</param>
        /// <param name="circle">검출된 원 중심 및 반지름</param>
        /// <returns>하단 반원 ROI 마스크</returns>
        public static Mat CreateSemicircleMask(int width, int height, CircleF circle)
        {
            var mask = new Mat(height, width, DepthType.Cv8U, 1);
            mask.SetTo(new MCvScalar(0));

            // 원 중심 및 반지름 정수화
            Point center = new Point(
                (int)System.Math.Round(circle.Center.X),
                (int)System.Math.Round(circle.Center.Y));
            int radius = (int)System.Math.Round(circle.Radius);

            // 1) 전체 원 채우기(255)
            CvInvoke.Circle(mask, center, radius, new MCvScalar(255), -1, LineType.AntiAlias);

            // 2) 원의 상단 절반 제거
            //    원 중심 Y 좌표 위쪽 영역을 0으로
            var rect = new Rectangle(0, 0, width, center.Y);
            CvInvoke.Rectangle(mask, rect, new MCvScalar(0), -1, LineType.AntiAlias);

            return mask;
        }
    }
}
