using System.Windows;
using FavaStudio.ViewModels;

namespace FavaStudio;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        var vm = new MainViewModel(Editor);
        DataContext = vm;
    }
}
