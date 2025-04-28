using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Emgu.CV;
using Tapex_Project.Services;
using System.Drawing;

public sealed class PreprocessingService : IPreprocessingService
{
    /// <summary>
    /// ImageJ Make Binary 와 비슷하게:
    /// 1) Otsu 자동 임계값 이진화
    /// 2) 열림+닫힘 연산으로 노이즈 제거 및 작은 구멍 메우기
    /// 3) 가장 큰 Blob Mask 추출
    /// </summary>
    public Mat ExtractLargestBlobMask(Mat gray8U)
    {
        // 1) Otsu Threshold (0 입력 → 자동 계산)
        var bin = new Mat();
        CvInvoke.Threshold(
            gray8U, bin,
            0, 255,
            ThresholdType.Binary | ThresholdType.Otsu);

        // 2) Morphology: 열림→닫힘 (커널 크기는 필요에 따라 조정)
        var kernel = CvInvoke.GetStructuringElement(
            ElementShape.Rectangle, new Size(5, 5), new Point(-1, -1));
        CvInvoke.MorphologyEx(bin, bin, MorphOp.Open, kernel, new Point(-1, -1), 1, BorderType.Default, new MCvScalar());
        CvInvoke.MorphologyEx(bin, bin, MorphOp.Close, kernel, new Point(-1, -1), 1, BorderType.Default, new MCvScalar());

        // 3) 최대 Blob Mask 반환
        return ExtractLargestBlobMask_Simple(bin);
    }

    // 기존 로직을 분리
    private Mat ExtractLargestBlobMask_Simple(Mat binMask)
    {
        using var contours = new VectorOfVectorOfPoint();
        CvInvoke.FindContours(binMask, contours, null,
                             RetrType.External, ChainApproxMethod.ChainApproxSimple);

        int idxMax = -1;
        double maxArea = 0;
        for (int i = 0; i < contours.Size; i++)
        {
            double area = CvInvoke.ContourArea(contours[i]);
            if (area > maxArea)
            {
                maxArea = area;
                idxMax = i;
            }
        }

        var mask = new Mat(binMask.Size, DepthType.Cv8U, 1);
        mask.SetTo(new MCvScalar(0));
        if (idxMax >= 0)
            CvInvoke.DrawContours(mask, contours, idxMax, new MCvScalar(255), -1);

        return mask;
    }
}
