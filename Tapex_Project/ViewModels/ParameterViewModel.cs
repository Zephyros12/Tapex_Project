using Tapex_Project.Models.Detection;

namespace Tapex_Project.ViewModels
{
    /// <summary>
    /// ParameterView의 ViewModel: DetectionConfig 노출
    /// </summary>
    public class ParameterViewModel : ViewModelBase
    {
        public DetectionConfig Config { get; } = new();
    }
}