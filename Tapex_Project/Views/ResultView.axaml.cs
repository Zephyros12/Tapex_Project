using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Tapex_Project.Views
{
    public partial class ResultView : UserControl
    {
        public ResultView()
        {
            InitializeComponent();
        }

        private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
    }
}
