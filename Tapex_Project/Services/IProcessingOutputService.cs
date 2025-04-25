using Emgu.CV;

namespace Tapex_Project.Services
{
    /// <summary>
    /// 검사 중간 결과(이미지)를 저장하는 기능 추상화
    /// </summary>
    public interface IProcessingOutputService
    {
        /// <summary>새 검사 사이클에 맞춰 디렉터리 초기화</summary>
        void Initialize();

        /// <summary>현재 검사 디렉터리에 Mat 이미지를 저장</summary>
        /// <param name="name">파일명</param>
        /// <param name="mat">저장할 Mat</param>
        void SaveMat(string name, Mat mat);
    }
}
