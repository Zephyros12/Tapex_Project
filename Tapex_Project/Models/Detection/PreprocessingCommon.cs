// Models/Detection/PreprocessingCommon.cs
using System;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Tapex_Project.Models.Detection;

namespace Tapex_Project.Models.Detection;

public static class PreprocessingCommon
{
    //public static Mat DetectAndFlatten(Mat srcBgr, CircleDetectionParam p)
    //{
    //    // 1) 그레이스케일 변환
    //    var gray = new Mat();
    //    CvInvoke.CvtColor(srcBgr, gray, ColorConversion.Bgr2Gray);

    //    // 2) 원(반원) 검출
    //    var circle = SemiCircleLocator.Detect(gray, p)
    //                 ?? throw new InvalidOperationException(
    //                    "반원 검출 실패: CircleDetectionParam 값을 조정하세요.");

    //    // 3) 하단 반원 마스크 생성 및 적용
    //    using var mask = RoiMask.CreateSemicircleMask(srcBgr.Width, srcBgr.Height, circle);
    //    var roiGray = new Mat();
    //    CvInvoke.BitwiseAnd(gray, gray, roiGray, mask);

    //    // 4) 배경 평탄화 (Top-Hat) - radiusPx = mm→px
    //    int radiusPx = (int)Math.Round(UnitHelper.MmToPx(p.FlattenRadiusMm));
    //    var flat = Preprocess.FlattenBackground(roiGray, radiusPx);

    //    return flat;
    //}
}
