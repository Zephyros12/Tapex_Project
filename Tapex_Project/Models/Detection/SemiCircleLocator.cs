// Models/Detection/SemiCircleLocator.cs
using System.Drawing;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;

namespace Tapex_Project.Models.Detection;

public static class SemiCircleLocator
{
    //public static CircleF? Detect(Mat gray8u, CircleDetectionParam p)
    //{
    //    // byte[] → px 간 변환
    //    int minDistPx = (int)System.Math.Round(UnitHelper.MmToPx(p.MinDistMm));
    //    int minRadPx = (int)System.Math.Round(UnitHelper.MmToPx(p.MinRadiusMm));
    //    int maxRadPx = (int)System.Math.Round(UnitHelper.MmToPx(p.MaxRadiusMm));

    //    using var circles = new VectorOfVectorOfPointF();
    //    // Emgu CV 의 HoughCircles 호출
    //    CvInvoke.HoughCircles(
    //        gray8u,
    //        circles,
    //        HoughModes.Gradient,
    //        p.Dp,
    //        minDistPx,
    //        (int)p.Param1,
    //        (int)p.Param2,
    //        minRadPx,
    //        maxRadPx);

    //    if (circles.Size > 0)
    //    {
    //        // VectorOfVectorOfPointF 의 첫 요소로부터 CircleF[] 획득
    //        var arr = circles.ToArrayOfArray();
    //        if (arr.Length > 0 && arr[0].Length > 0)
    //            return arr[0][0];
    //    }

    //    return null;
    //}
}
