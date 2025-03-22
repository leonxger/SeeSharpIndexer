using System.Windows;
using System.Windows.Input;

namespace SeeSharpIndexer;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        
        // Handle keyboard commands globally
        this.CommandBindings.Add(new CommandBinding(ApplicationCommands.New, (s, e) => {
            if (DataContext is ViewModels.MainViewModel vm && vm.AddFilesCommand.CanExecute(null))
                vm.AddFilesCommand.Execute(null);
        }));
        
        this.CommandBindings.Add(new CommandBinding(ApplicationCommands.Open, (s, e) => {
            if (DataContext is ViewModels.MainViewModel vm && vm.LoadIndexCommand.CanExecute(null))
                vm.LoadIndexCommand.Execute(null);
        }));
        
        this.CommandBindings.Add(new CommandBinding(ApplicationCommands.Save, (s, e) => {
            if (DataContext is ViewModels.MainViewModel vm && vm.SaveIndexCommand.CanExecute(null))
                vm.SaveIndexCommand.Execute(null);
        }));
        
        // Set focus to the main content area
        this.Loaded += (s, e) => {
            Keyboard.Focus(this);
        };
    }
    
    protected override void OnPreviewKeyDown(System.Windows.Input.KeyEventArgs e)
    {
        base.OnPreviewKeyDown(e);
        
        // Handle F5 key for scan
        if (e.Key == Key.F5 && DataContext is ViewModels.MainViewModel vm)
        {
            if (vm.ScanCommand.CanExecute(null))
            {
                vm.ScanCommand.Execute(null);
                e.Handled = true;
            }
        }
        
        // Handle Escape key to close application
        if (e.Key == Key.Escape)
        {
            Close();
        }
    }
    
    // Allow dragging the window by clicking anywhere on the title bar
    private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed)
        {
            DragMove();
        }
    }
    
    // Close button click handler
    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {

    }
}