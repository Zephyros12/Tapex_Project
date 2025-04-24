namespace Tapex_Project.Models;

public sealed class DetectorConfig
{
    public int AdaptiveBlockSize { get; set; } = 51;
    public int AdaptiveC { get; set; } = 10;
    public int MorphKernel { get; set; } = 3;
    public int MinAreaPx { get; set; } = 50;
    public int MaxAreaPx { get; set; } = 10_000;
    public double MinCircularity { get; set; } = 0.3;
}
