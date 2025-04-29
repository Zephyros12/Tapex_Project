namespace Tapex_Project.Models.Detection
{
    /// <summary>
    /// 전체 검사 파라미터를 담는 설정 클래스
    /// </summary>
    public sealed class DetectionConfig
    {
        /// <summary>
        /// 한 픽셀의 크기 (마이크로미터 단위)
        /// </summary>
        public double PixelSizeMicrometer { get; } = 21.0;

        /// <summary>최소 ROI 가로 길이 (mm)</summary>
        public double MinDefectWidthMm { get; set; } = 0.1;

        /// <summary>최대 ROI 가로 길이 (mm)</summary>
        public double MaxDefectWidthMm { get; set; } = 100.0;

        /// <summary>최소 ROI 세로 길이 (mm)</summary>
        public double MinDefectHeightMm { get; set; } = 0.1;

        /// <summary>최대 ROI 세로 길이 (mm)</summary>
        public double MaxDefectHeightMm { get; set; } = 100.0;

        /// <summary>
        /// 이진화 임계값 (밝기)
        /// </summary>
        public double BrightnessThreshold { get; set; } = 128.0;

        /// <summary>
        /// 선명도(Sharpness) 임계값
        /// </summary>
        public double SharpnessThreshold { get; set; } = 100.0;

        /// <summary>
        /// 버블(기포) 전처리 및 검출 파라미터
        /// </summary>
        public BubbleParam Bubble { get; } = new();

        /// <summary>
        /// 스크래치(긁힘) 전처리 및 검출 파라미터
        /// </summary>
        public ScratchParam Scratch { get; } = new();

        /// <summary>
        /// 먼지(Dust) 전처리 및 검출 파라미터
        /// </summary>
        public DustParam Dust { get; } = new();

        /// <summary>
        /// 크랙(Crack) 전처리 및 검출 파라미터
        /// </summary>
        public CrackParam Crack { get; } = new();
    }

    /// <summary>
    /// 버블(기포) 검출을 위한 필터링 및 임계값 설정
    /// </summary>
    public sealed class BubbleParam
    {
        /// <summary>
        /// Morphology 커널 크기 (홀수 권장)
        /// </summary>
        public int MorphKernelSize { get; set; } = 5;

        /// <summary>
        /// 이진화 임계값 (Threshold)
        /// </summary>
        public double Threshold { get; set; } = 128.0;

        /// <summary>
        /// 이진화 후 최대값 (MaxValue)
        /// </summary>
        public double MaxValue { get; set; } = 255.0;

        /// <summary>
        /// 최소 면적 필터링 (픽셀 단위)
        /// </summary>
        public double MinArea { get; set; } = 50.0;

        /// <summary>
        /// 최소 너비 필터링 (픽셀 단위)
        /// </summary>
        public int MinWidth { get; set; } = 5;

        /// <summary>
        /// 최소 높이 필터링 (픽셀 단위)
        /// </summary>
        public int MinHeight { get; set; } = 5;

        /// <summary>
        /// 최소 원형도 필터링 (0.0 ~ 1.0)
        /// </summary>
        public double MinCircularity { get; set; } = 0.5;
    }

    /// <summary>
    /// 스크래치(긁힘) 검출을 위한 엣지 및 선 검출 파라미터
    /// </summary>
    public sealed class ScratchParam
    {
        /// <summary>
        /// Canny Edge 검출 임계값1 (하위)
        /// </summary>
        public double CannyThreshold1 { get; set; } = 50.0;

        /// <summary>
        /// Canny Edge 검출 임계값2 (상위)
        /// </summary>
        public double CannyThreshold2 { get; set; } = 150.0;

        /// <summary>
        /// 팽창(Dilate) 커널 크기 (픽셀 단위)
        /// </summary>
        public int DilateKernel { get; set; } = 3;

        /// <summary>
        /// 최소 선 길이 필터링 (mm 단위)
        /// </summary>
        public double MinLengthMm { get; set; } = 1.0;

        /// <summary>
        /// 최대 선 너비 필터링 (픽셀 단위)
        /// </summary>
        public int MaxWidthPx { get; set; } = 5;

        /// <summary>
        /// HoughLinesP 함수의 threshold 값
        /// </summary>
        public int HoughThreshold { get; set; } = 50;

        /// <summary>
        /// HoughLinesP 최소 선 길이 (mm 단위)
        /// </summary>
        public double MinLineLengthMm { get; set; } = 2.0;

        /// <summary>
        /// HoughLinesP 최대 선 간격 (mm 단위)
        /// </summary>
        public double MaxLineGapMm { get; set; } = 1.0;
    }

    /// <summary>
    /// 먼지(Dust) 검출을 위한 이진화 및 영역 필터링 파라미터
    /// </summary>
    public sealed class DustParam
    {
        /// <summary>
        /// 이진화 임계값 (Threshold)
        /// </summary>
        public double Threshold { get; set; } = 50.0;

        /// <summary>
        /// 최소 면적 필터링 (픽셀 단위)
        /// </summary>
        public double MinArea { get; set; } = 5.0;

        /// <summary>
        /// 최대 면적 필터링 (픽셀 단위)
        /// </summary>
        public double MaxArea { get; set; } = 200.0;

        /// <summary>
        /// Morphology 커널 크기 (픽셀 단위)
        /// </summary>
        public int MorphKernel { get; set; } = 3;
    }

    /// <summary>
    /// 크랙(Crack) 검출을 위한 엣지 및 선 검출 파라미터
    /// </summary>
    public sealed class CrackParam
    {
        /// <summary>
        /// Canny Edge 검출 임계값1 (하위)
        /// </summary>
        public double CannyThreshold1 { get; set; } = 100.0;

        /// <summary>
        /// Canny Edge 검출 임계값2 (상위)
        /// </summary>
        public double CannyThreshold2 { get; set; } = 200.0;

        /// <summary>
        /// HoughLinesP 함수의 threshold 값
        /// </summary>
        public int HoughThreshold { get; set; } = 50;

        /// <summary>
        /// HoughLinesP 최소 선 길이 (mm 단위)
        /// </summary>
        public double MinLineLengthMm { get; set; } = 5.0;

        /// <summary>
        /// HoughLinesP 최대 선 간격 (mm 단위)
        /// </summary>
        public double MaxLineGapMm { get; set; } = 2.0;

        /// <summary>
        /// 선 너비 필터링 (픽셀 단위)
        /// </summary>
        public int LineWidthPx { get; set; } = 3;
    }
}
