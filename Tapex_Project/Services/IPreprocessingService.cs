using Emgu.CV;

namespace Tapex_Project.Services
{
    /// <summary>
    /// 이진화 마스크에서 가장 큰 흰색 덩어리만 남긴 마스크를 생성하는 서비스 인터페이스
    /// </summary>
    public interface IPreprocessingService
    {
        Mat ExtractLargestBlobMask(Mat binMask);
    }
}
