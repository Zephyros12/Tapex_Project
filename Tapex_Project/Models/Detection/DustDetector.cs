using System.Collections.Generic;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Tapex_Project.Models;
using Tapex_Project.Services;
using System.Drawing;
using System;

namespace Tapex_Project.Models.Detection
{
    /// <summary>
    /// 먼지(Dust) 전처리 및 검출기:
    /// 동일 임계값으로 밝은(흰) 점과 어두운(검은) 점을 모두 찾습니다.
    /// </summary>
    public sealed class DustDetector : ISubDetector
    {
        public IReadOnlyList<DefectResult> Run(Mat srcGray, DetectionConfig cfg)
        {
            // 0) 설정 및 결과 리스트 초기화
            var p = cfg.Dust;
            var results = new List<DefectResult>();
            // 0) Structuring Element 정의
            var elem = CvInvoke.GetStructuringElement(
                ElementShape.Ellipse, new Size(15, 15), new Point(-1, -1));

            // 1) Top-Hat → 밝은 먼지 강조
            Mat topHat = new Mat();
            CvInvoke.MorphologyEx(
                srcGray, topHat,
                MorphOp.Tophat, elem, new Point(-1, -1),
                1, BorderType.Reflect101, new MCvScalar());

            // 2) Black-Hat → 어두운 먼지 강조
            Mat blackHat = new Mat();
            CvInvoke.MorphologyEx(
                srcGray, blackHat,
                MorphOp.Blackhat, elem, new Point(-1, -1),
                1, BorderType.Reflect101, new MCvScalar());

            // 3) 임계값(Threshold)으로 이진화
            Mat binTop = new Mat(), binBlack = new Mat();
            CvInvoke.Threshold(topHat, binTop, 30, 255, ThresholdType.Binary);
            CvInvoke.Threshold(blackHat, binBlack, 30, 255, ThresholdType.Binary);
            CvInvoke.Imwrite("result_binTop.png", binTop);
            CvInvoke.Imwrite("result_binBlack.png", binBlack);
            // 4) 두 마스크를 OR 연산
            Mat binHybrid = new Mat();
            CvInvoke.BitwiseOr(binTop, binBlack, binHybrid);
            CvInvoke.Imwrite("result.png", binHybrid);
            // 5) 작은 노이즈 제거용 열림 연산
            var smallElem = CvInvoke.GetStructuringElement(
                ElementShape.Ellipse, new Size(3, 3), new Point(-1, -1));
            CvInvoke.MorphologyEx(
                binHybrid, binHybrid,
                MorphOp.Open, smallElem, new Point(-1, -1),
                1, BorderType.Reflect101, new MCvScalar());

            // 6) 컨투어 검출 및 그리기
            var contours = new VectorOfVectorOfPoint();
            CvInvoke.FindContours(
                binHybrid, contours, null,
                RetrType.External, ChainApproxMethod.ChainApproxSimple);

          
            for (int i = 0; i < contours.Size; i++)
            {
                var cnt = contours[i];
                double area = CvInvoke.ContourArea(cnt);
                if (area < p.MinArea || area > p.MaxArea)
                    continue;

                var rect = CvInvoke.BoundingRectangle(cnt);

                // ROI 크롭
                var preCrop = new Mat(binHybrid, rect);
                var preBmp = DetectorHelpers.ConvertMatToBitmap(preCrop);

                results.Add(new DefectResult
                {
                    X = rect.X,
                    Y = rect.Y,
                    Width = rect.Width,
                    Height = rect.Height,
                    Type = DefectType.Dust,
                    PreprocessedImage = preBmp
                });
            }
            

            return results;
        }
    }
}
