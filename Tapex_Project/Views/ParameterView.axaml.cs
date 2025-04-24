using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Tapex_Project.Views
{
    public partial class ParameterView : UserControl
    {
        public ParameterView()
        {
            InitializeComponent();
        }

        private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
    }
}