using System;
using System.Collections.Generic;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Tapex_Project.Models;
using Tapex_Project.Services;
using System.Drawing;

namespace Tapex_Project.Models.Detection
{
    public sealed class BubbleDetector : ISubDetector
    {
        private readonly IProcessingOutputService _outputService;

        public BubbleDetector(IProcessingOutputService outputService)
        {
            _outputService = outputService;
        }

        public Mat? MorphKernel { get; set; }

        public IReadOnlyList<DefectResult> Run(Mat srcGray, DetectionConfig cfg)
        {
            var p = cfg.Bubble;
            var results = new List<DefectResult>();


         
            // 1) 배경 굴곡 제거 (수직방향으로 블러)
            var bg = new Mat();
            CvInvoke.GaussianBlur(srcGray, bg, new Size(300, 300), 0); // 기본 blurKernel 크기 사용
            var noCurl = new Mat();
            CvInvoke.Subtract(srcGray, bg, noCurl);

            // 2) 줄무늬 평탄화 (열별 평균 → flat-field)
            var profile = new Mat();
            CvInvoke.Reduce(noCurl, profile, ReduceDimension.SingleRow, ReduceType.ReduceAvg, DepthType.Cv32F);
            var colBg = new Mat();
            CvInvoke.Repeat(profile, noCurl.Rows, 1, colBg);

            var noCurlF = new Mat();
            noCurl.ConvertTo(noCurlF, DepthType.Cv32F);

            var flatField = new Mat();
            CvInvoke.Divide(noCurlF, colBg, flatField);
            double meanVal = CvInvoke.Mean(noCurlF).V0;
            CvInvoke.Multiply(flatField, new ScalarArray(meanVal), flatField);

            var flat8u = new Mat();
            flatField.ConvertTo(flat8u, DepthType.Cv8U);

            // 3) CLAHE (국부 대비 강화)
            var claheResult = new Mat();
            CvInvoke.CLAHE(flat8u, 2, new Size(8, 8), claheResult); // p.ClaheClip 사용

            // 4) Top-Hat 강조 (원본 – Opening)
            var elem = CvInvoke.GetStructuringElement(
                ElementShape.Ellipse,
                new Size(20 * 3 + 1, 20 * 3 + 1),
                new Point(-1, -1));

            var tophat = new Mat();
            CvInvoke.MorphologyEx(claheResult, tophat, MorphOp.Tophat, elem, new Point(-1, -1), 1, BorderType.Reflect101, new MCvScalar());

            // 5) Otsu 이진화 + Morphology 연산 (Opening + Closing)
            var binary = new Mat();
            CvInvoke.Threshold(tophat, binary, 0, 255, ThresholdType.Binary | ThresholdType.Otsu);

            // Morphology: Opening + Closing
            var openElem = CvInvoke.GetStructuringElement(ElementShape.Ellipse, new Size(2, 2), new Point(-1, -1));
            CvInvoke.MorphologyEx(binary, binary, MorphOp.Open, openElem, new Point(-1, -1), 1, BorderType.Reflect101, new MCvScalar());

            var closeElem = CvInvoke.GetStructuringElement(ElementShape.Ellipse, new Size(8, 8), new Point(-1, -1));
            CvInvoke.MorphologyEx(binary, binary, MorphOp.Close, closeElem, new Point(-1, -1), 1, BorderType.Reflect101, new MCvScalar());

            // 작은 영역 제거
            CvInvoke.MorphologyEx(binary, binary, MorphOp.Open, openElem, new Point(-1, -1), 5, BorderType.Reflect101, new MCvScalar());


            CvInvoke.Imwrite("binary1.png", binary);
            var contours = new VectorOfVectorOfPoint();

            CvInvoke.FindContours(binary, contours, null, RetrType.External, ChainApproxMethod.ChainApproxSimple);

            // 4) 기포 검출
            for (int i = 0; i < contours.Size; i++)
            {
                var contour = contours[i];
                double area = CvInvoke.ContourArea(contour);
                if (area < 2000) continue;

                var rect = CvInvoke.BoundingRectangle(contour);
                //if (rect.Width < p.MinWidth || rect.Height < p.MinHeight)
                //    continue;



                // 5) ROI 추출 및 검정색 비율 계산
                Mat roi = new Mat(binary, rect);
                // ROI에서 흰색 부분 계산 (255 값)
                int whiteCount = CvInvoke.CountNonZero(roi); // 흰색 픽셀의 수
                double whiteRatio = whiteCount / roi.Total; // 흰색 비율 계산

                // ROI에서 검정색 부분 계산 (0 값)
                Mat blackMat = new Mat();
                CvInvoke.BitwiseNot(roi, blackMat);  // 흰색을 검정색으로 변환
                int blackCount = CvInvoke.CountNonZero(blackMat); // 검정색 픽셀의 수
                double blackRatio = blackCount / roi.Total; // 검정색 비율 계산

                // 검정색 비율이 일정 값 이상이면 ROI를 그리도록 조건 추가
                double blackThreshold = 0.9; // 검정색 비율 기준 (예: 90%)
                if (blackRatio > blackThreshold)
                {
                    // 기포 정보 추가
                    results.Add(new DefectResult
                    {
                        X = rect.X,
                        Y = rect.Y,
                        Width = rect.Width,
                        Height = rect.Height,
                        DistanceFromEdge = Math.Min(
                            Math.Min(rect.X, srcGray.Width - rect.Right),
                            Math.Min(rect.Y, srcGray.Height - rect.Bottom)),
                       // Score = circ,
                        Type = DefectType.Bubble
                    });
                }
            
        }

            return results;
        }
    }
}
