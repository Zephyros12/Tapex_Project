using System;
using System.IO;
using Emgu.CV;

namespace Tapex_Project.Services
{
    /// <summary>
    /// 로컬 파일시스템에 ProcessingOutputs 디렉터리를 관리하고 이미지 저장을 담당
    /// </summary>
    public class ProcessingOutputService : IProcessingOutputService
    {
        private readonly string _baseDir;
        private string _runDir = string.Empty;

        public ProcessingOutputService()
        {
            _baseDir = Path.Combine(AppContext.BaseDirectory, "ProcessingOutputs");
        }

        public void Initialize()
        {
            Directory.CreateDirectory(_baseDir);
            _runDir = Path.Combine(_baseDir, DateTime.Now.ToString("yyyyMMdd_HHmmss"));
            Directory.CreateDirectory(_runDir);
        }

        public void SaveMat(string name, Mat mat)
        {
            var path = Path.Combine(_runDir, name);
            CvInvoke.Imwrite(path, mat);
        }
    }
}
