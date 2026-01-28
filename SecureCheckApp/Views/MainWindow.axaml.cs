using Avalonia.Controls;
using SecureCheckApp.ViewModels;

namespace SecureCheckApp.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel();
    }
}
