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

        // Auto-scroll the console TextBox whenever ConsoleOutput changes
        vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(vm.ConsoleOutput))
                ConsoleBox.ScrollToEnd();
        };
    }
}
