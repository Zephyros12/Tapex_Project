namespace Tapex_Project.Models.Detection
{
    public sealed class DetectionConfig
    {
        public CircleDetectionParam Circle { get; } = new();
        public BubbleParam Bubble { get; } = new();
        public ScratchParam Scratch { get; } = new();
        public DustParam Dust { get; } = new();
        public CrackParam Crack { get; } = new();
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
        public double MinDiameterMm { get; set; } = 0.20;
        public double MaxDiameterMm { get; set; } = 3.00;
        public double MinCircularity { get; set; } = 0.70;
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
