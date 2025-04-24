using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Tapex_Project.Services;
using Tapex_Project.ViewModels;

namespace Tapex_Project.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            DataContext = new MainWindowViewModel(new FilePickerServiceAvalonia(this));
        }

        private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
    }
}