namespace Tapex_Project.Models
{
    public enum DefectType
    {
        Unknown,
        Bubble,
        Scratch,
        Dust,
        Crack
    }

    public sealed class DefectResult
    {
        // 위치 및 크기 (픽셀 단위)
        public double X { get; init; }
        public double Y { get; init; }
        public double Width { get; init; }
        public double Height { get; init; }

        // 가장자리로부터 거리 (픽셀 단위)
        public double DistanceFromEdge { get; init; }

        // 검출 스코어 (예: 원형도, 길이 등)
        public double Score { get; init; }

        // 불량 유형
        public DefectType Type { get; init; }
    }
}