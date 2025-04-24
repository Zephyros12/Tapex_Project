using System;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Tapex_Project.Models.Detection;

namespace Tapex_Project.Models.Detection;

public static partial class PreprocessingCommon
{
    public static Mat FlattenGlobal(Mat gray8u, double flattenRadiusMm)
    {
        int radiusPx = (int)Math.Round(UnitHelper.MmToPx(flattenRadiusMm));
        return Preprocess.FlattenBackground(gray8u, radiusPx);
    }
}

