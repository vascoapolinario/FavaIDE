using System.Windows;
using System.Linq;
using FavaStudio.Editor;
using FavaStudio.Models;
using FavaStudio.ViewModels;

namespace FavaStudio;

public partial class MainWindow : Window
{
    private readonly DiagnosticUnderlineRenderer _diagnosticUnderlineRenderer;

    public MainWindow()
    {
        InitializeComponent();
        _diagnosticUnderlineRenderer = new DiagnosticUnderlineRenderer(Editor);
        Editor.TextArea.TextView.BackgroundRenderers.Add(_diagnosticUnderlineRenderer);

        var vm = new MainViewModel(Editor);
        DataContext = vm;

        // Auto-scroll VM output when it updates
        vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(vm.VmOutput))
                VmOutputBox.ScrollToEnd();
        };
        vm.Diagnostics.CollectionChanged += (_, _) => _diagnosticUnderlineRenderer.SetDiagnostics(vm.Diagnostics.ToList());
        _diagnosticUnderlineRenderer.SetDiagnostics(vm.Diagnostics.ToList());
    }

    private void ProjectTree_OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (DataContext is MainViewModel vm && e.NewValue is ProjectNode node)
            vm.SetSelectedProjectNode(node);
    }
}
