namespace Tapex_Project.Models.Detection
{
    public sealed class DetectionConfig
    {
        public CircleDetectionParam Circle { get; } = new();
        public BubbleParam Bubble { get; } = new();
        public ScratchParam Scratch { get; } = new();
        public DustParam Dust { get; } = new();
        public CrackParam Crack { get; } = new();
        public double PixelSizeMicrometer { get; } = 21.0;
        public double MinDefectSizeMm { get; set; } = 0.1;
        public double BrightnessThreshold { get; set; } = 128.0;
        public double SharpnessThreshold { get; set; } = 100.0;
    }

    public sealed class CircleDetectionParam
    {
        public double Dp { get; set; } = 1.2;
        public double MinDistMm { get; set; } = 100.0;
        public double Param1 { get; set; } = 200.0;
        public double Param2 { get; set; } = 100.0;
        public double MinRadiusMm { get; set; } = 172.0;
        public double MaxRadiusMm { get; set; } = 180.0;
        public double FlattenRadiusMm { get; set; } = 3.0;
    }

    public sealed class BubbleParam
    {
        public int MorphKernelSize { get; set; } = 3;
        public double Threshold { get; set; } = 100.0;
        public double MaxValue { get; set; } = 255.0;
        public double MinArea { get; set; } = 50.0;
        public int MinWidth { get; set; } = 5;
        public int MinHeight { get; set; } = 5;
        public double MinCircularity { get; set; } = 0.7;
    }

    public sealed class ScratchParam
    {
        // 추후 구현: 예) public double MinLengthMm { get; set; }
    }

    public sealed class DustParam
    {
        // 추후 구현: 예) public double MaxDiameterMm { get; set; }
    }

    public sealed class CrackParam
    {
        // 추후 구현: 예) public double MinLengthMm { get; set; }
    }
}
