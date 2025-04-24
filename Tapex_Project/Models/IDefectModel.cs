namespace Tapex_Project.Models;

public interface IDefectModel
{
    double X { get; }
    double Y { get; }
    double Width { get; }
    double Height { get; }
    double DistanceFromEdge { get; }
    DefectType Type { get; }
}

public enum DefectType
{
    Unknown = 0,
    Dust,
    Bubble,
    Scratch,
    Crack
}
