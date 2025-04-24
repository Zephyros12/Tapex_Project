using System;
using System.Drawing;
using System.Linq;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;

namespace Tapex_Project.Models.Detection
{
    /// <summary>
    /// 이미지 내 잘린 반원 형태의 컨투어를 찾아,
    /// 최소 외접원(circle)을 계산하는 도우미 클래스
    /// </summary>
    public static class SemiCircleLocator
    {
        /// <summary>
        /// 그레이스케일 Mat에서 GaussianBlur + Canny → 컨투어 → 최소 외접원을 구합니다.
        /// 잘린 반원의 궤적만으로 실제 원의 중심과 반지름을 근사합니다.
        /// </summary>
        /// <param name="srcGray">8bit 단일 채널 그레이스케일 영상</param>
        /// <returns>반원 외접원 CircleF 또는 null</returns>
        public static CircleF? DetectClippedCircle(Mat srcGray)
        {
            // 1) 노이즈 제거용 블러
            using var blur = new Mat();
            CvInvoke.GaussianBlur(srcGray, blur, new Size(5, 5), 1.5);

            // 2) Canny 엣지 검출
            using var edges = new Mat();
            CvInvoke.Canny(blur, edges, 50, 150);

            // 3) 컨투어 찾기
            using var contours = new VectorOfVectorOfPoint();
            CvInvoke.FindContours(
                edges, contours, null,
                RetrType.External, ChainApproxMethod.ChainApproxSimple);

            if (contours.Size == 0)
                return null;

            // 4) 가장 긴 컨투어 선택
            int bestIdx = 0;
            double maxLen = 0;
            for (int i = 0; i < contours.Size; i++)
            {
                double len = CvInvoke.ArcLength(contours[i], false);
                if (len > maxLen)
                {
                    maxLen = len;
                    bestIdx = i;
                }
            }

            var bestContour = contours[bestIdx];

            // 5) Point 배열 → PointF 배열
            Point[] pts = bestContour.ToArray();
            PointF[] ptsF = pts.Select(p => new PointF(p.X, p.Y)).ToArray();

            using var vecF = new VectorOfPointF(ptsF);

            // 6) 최소 외접원 계산
            CircleF circle = CvInvoke.MinEnclosingCircle(vecF);
            return circle;
        }
    }
}