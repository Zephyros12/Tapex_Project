using Avalonia.Controls;
using Tapex_Project.Services;
using Tapex_Project.ViewModels;

namespace Tapex_Project.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        var picker = new FilePickerServiceAvalonia(this);
        DataContext = new MainWindowViewModel(picker);
    }
}
