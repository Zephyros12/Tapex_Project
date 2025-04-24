namespace Tapex_Project.Models;

public sealed class DefectResult : IDefectModel
{
    public double X { get; init; }
    public double Y { get; init; }
    public double Width { get; init; }
    public double Height { get; init; }
    public double DistanceFromEdge { get; init; }
    public DefectType Type { get; init; } = DefectType.Unknown;
    public double Score { get; init; }
}
