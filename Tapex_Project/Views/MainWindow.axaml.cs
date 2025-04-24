using Avalonia.Controls;
using Tapex_Project.ViewModels;

namespace Tapex_Project.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel();
    }
}
