// 파일: Models/Detection/DustDetector.cs
using System;
using System.Collections.Generic;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Tapex_Project.Models;
using Tapex_Project.Services;

namespace Tapex_Project.Models.Detection
{
    /// <summary>
    /// 먼지(Dust) 전처리 및 검출기:
    /// 동일 임계값으로 밝은(흰) 점과 어두운(검은) 점을 모두 찾습니다.
    /// </summary>
    public sealed class DustDetector : ISubDetector
    {
        private readonly IProcessingOutputService _outputService;

        public DustDetector(IProcessingOutputService outputService)
        {
            _outputService = outputService;
        }

        public IReadOnlyList<DefectResult> Run(Mat srcGray, DetectionConfig cfg)
        {
            var p = cfg.Dust;
            var results = new List<DefectResult>();

            // 1) 밝은 점 이진화 (ThresholdType.Binary)
            using var binLight = new Mat();
            CvInvoke.Threshold(
                srcGray,
                binLight,
                p.Threshold,
                255,
                ThresholdType.Binary);
            _outputService.SaveMat("Dust_01_Binary_Light.png", binLight);

            // 2) 어두운 점 이진화 (ThresholdType.BinaryInv)
            using var binDark = new Mat();
            CvInvoke.Threshold(
                srcGray,
                binDark,
                p.Threshold,
                255,
                ThresholdType.BinaryInv);
            _outputService.SaveMat("Dust_02_Binary_Dark.png", binDark);

            // 3) 밝은/어두운 마스크 합치기
            using var bin = new Mat();
            CvInvoke.BitwiseOr(binLight, binDark, bin);
            _outputService.SaveMat("Dust_03_Combined.png", bin);

            // 4) Morphology (노이즈 제거용, 선택 사항)
            var kernel = CvInvoke.GetStructuringElement(
                ElementShape.Rectangle,
                new System.Drawing.Size(p.MorphKernel, p.MorphKernel),
                new System.Drawing.Point(-1, -1));
            CvInvoke.MorphologyEx(
                bin, bin,
                MorphOp.Open,
                kernel,
                new System.Drawing.Point(-1, -1),
                1,
                BorderType.Default,
                new MCvScalar());
            _outputService.SaveMat("Dust_04_MorphOpen.png", bin);

            // 5) 컨투어 검출 & 면적 필터링
            using var contours = new VectorOfVectorOfPoint();
            CvInvoke.FindContours(
                bin, contours, null,
                RetrType.External,
                ChainApproxMethod.ChainApproxSimple);

            for (int i = 0; i < contours.Size; i++)
            {
                double area = CvInvoke.ContourArea(contours[i]);
                if (area < p.MinArea || area > p.MaxArea)
                    continue;

                var rect = CvInvoke.BoundingRectangle(contours[i]);
                results.Add(new DefectResult
                {
                    X = rect.X,
                    Y = rect.Y,
                    Width = rect.Width,
                    Height = rect.Height,
                    Type = DefectType.Dust
                });
            }

            return results;
        }
    }
}
