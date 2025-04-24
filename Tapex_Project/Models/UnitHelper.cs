namespace Tapex_Project.Models;

public static class UnitHelper
{
    public const double PixelPitchMm = 0.021;   // 21 µm

    public static double PxToMm(double px) => px * PixelPitchMm;
    public static double MmToPx(double mm) => mm / PixelPitchMm;
}
