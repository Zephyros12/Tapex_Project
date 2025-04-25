using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Tapex_Project.Views
{
    public partial class DetailView : UserControl
    {
        public DetailView()
        {
            InitializeComponent();
        }

        private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
    }
}
