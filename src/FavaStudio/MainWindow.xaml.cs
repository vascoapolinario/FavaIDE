using System.Windows;
using FavaStudio.Models;
using FavaStudio.ViewModels;

namespace FavaStudio;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        var vm = new MainViewModel(Editor);
        DataContext = vm;

        // Auto-scroll VM output when it updates
        vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(vm.VmOutput))
                VmOutputBox.ScrollToEnd();
        };
    }

    private void ProjectTree_OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (DataContext is MainViewModel vm && e.NewValue is ProjectNode node)
            vm.SetSelectedProjectNode(node);
    }
}
