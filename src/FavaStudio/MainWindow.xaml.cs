using System.Windows;
using System.Windows.Controls;
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

    private static ProjectNode? GetContextNode(object sender)
    {
        if (sender is not MenuItem menuItem) return null;
        if (menuItem.Parent is not ContextMenu contextMenu) return null;
        return (contextMenu.PlacementTarget as FrameworkElement)?.DataContext as ProjectNode;
    }

    private void NewFavaFileFromTree_OnClick(object sender, RoutedEventArgs e)
        => ExecuteNodeAction(sender, vm => vm.CreateNewFavaAtSelectedNode());

    private void NewTextFileFromTree_OnClick(object sender, RoutedEventArgs e)
        => ExecuteNodeAction(sender, vm => vm.CreateNewTextAtSelectedNode());

    private void DeleteTreeNode_OnClick(object sender, RoutedEventArgs e)
    {
        ExecuteNodeAction(sender, vm => vm.DeleteSelectedNode());
    }

    private void ExecuteNodeAction(object sender, Action<MainViewModel> action)
    {
        var node = GetContextNode(sender);
        if (DataContext is not MainViewModel vm || node is null) return;

        vm.SetSelectedProjectNode(node);
        action(vm);
    }
}
