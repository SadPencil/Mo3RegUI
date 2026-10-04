using Mo3RegUI.MvvmContract;
using System.Windows;

namespace Mo3RegUI.View
{
    /// <summary>
    /// The main window. It makes no decisions of its own: it binds every value it shows to the
    /// injected <see cref="IMainWindowViewModel"/> and invokes its commands on user gestures. The
    /// ViewModel is reached only through the contract interface, and the window lifecycle is
    /// reported back through <c>IViewLifecycleService</c> (attached by the composition root), so
    /// there is no code in this file beyond wiring the DataContext.
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow(IMainWindowViewModel viewModel)
        {
            this.InitializeComponent();
            this.DataContext = viewModel;
        }
    }
}
