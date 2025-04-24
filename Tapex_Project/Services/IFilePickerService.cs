using System.Collections.Generic;
using System.Threading.Tasks;

namespace Tapex_Project.Services
{
    /// <summary>
    /// 파일 선택 다이얼로그를 통해 이미지 파일 경로 목록을 반환하는 서비스 인터페이스
    /// </summary>
    public interface IFilePickerService
    {
        /// <summary>
        /// 이미지 파일을 선택할 수 있는 다이얼로그를 표시하고, 선택된 파일 경로 리스트를 반환합니다.
        /// </summary>
        Task<List<string>> PickImageFilesAsync();
    }
}