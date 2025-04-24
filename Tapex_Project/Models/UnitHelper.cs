namespace Tapex_Project.Models
{
    /// <summary>
    /// 픽셀 ↔ 물리 단위(mm) 변환 헬퍼
    /// </summary>
    public static class UnitHelper
    {
        /// <summary>픽셀 피치 (mm 단위): 21 µm = 0.021 mm</summary>
        public const double PixelPitchMm = 0.021;

        /// <summary>픽셀 → mm</summary>
        public static double PxToMm(double px) => px * PixelPitchMm;

        /// <summary>mm → 픽셀</summary>
        public static double MmToPx(double mm) => mm / PixelPitchMm;
    }
}