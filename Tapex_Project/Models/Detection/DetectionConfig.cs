namespace Tapex_Project.Models.Detection
{
    public sealed class DetectionConfig
    {
        public double PixelSizeMicrometer { get; } = 21.0;
        public double MinDefectSizeMm { get; set; } = 0.1;
        public double BrightnessThreshold { get; set; } = 128.0;
        public double SharpnessThreshold { get; set; } = 100.0;

        public BubbleParam Bubble { get; } = new();
        public ScratchParam Scratch { get; } = new();
        public DustParam Dust { get; } = new();
        public CrackParam Crack { get; } = new();
    }

    public sealed class BubbleParam
    {
        public int MorphKernelSize { get; set; } = 5;
        public double Threshold { get; set; } = 128.0;
        public double MaxValue { get; set; } = 255.0;
        public double MinArea { get; set; } = 50.0;
        public int MinWidth { get; set; } = 5;
        public int MinHeight { get; set; } = 5;
        public double MinCircularity { get; set; } = 0.5;
    }

    public sealed class ScratchParam
    {
        public double CannyThreshold1 { get; set; } = 50.0;
        public double CannyThreshold2 { get; set; } = 150.0;
        public int DilateKernel {  get; set; } = 3;
        public double MinLengthMm { get; set; } = 1.0;
        public int MaxWidthPx { get; set; } = 5;
    }

    public sealed class DustParam
    {
        public double Threshold { get; set; } = 50.0;
        public double MinArea { get; set; } = 5.0;
        public double MaxArea { get; set; } = 200.0;
        public int MorphKernel { get; set; } = 3;
    }

    public sealed class CrackParam
    {
        // 추후 구현: 예) public double MinLengthMm { get; set; }
    }
}
