using Tapex_Project.Models.Detection;

namespace Tapex_Project.ViewModels
{
    /// <summary>
    /// ParameterView의 ViewModel: DetectionConfig 노출
    /// </summary>
    public class ParameterViewModel : ViewModelBase
    {
        public DetectionConfig Config { get; }

        public ParameterViewModel()
        {
            Config = new DetectionConfig();
        }

        public BubbleParam BubbleParams => Config.Bubble;
        public DustParam DustParams => Config.Dust;
        public ScratchParam ScratchParams => Config.Scratch;
        public CrackParam CrackParams => Config.Crack;
    }
}