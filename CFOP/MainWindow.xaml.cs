using System.ComponentModel;
using System.Windows;

namespace CFOP
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel;

        public MainWindow(MainViewModel viewModel)
        {
            InitializeComponent();

            _viewModel = viewModel;
            DataContext = viewModel;            
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            _viewModel.Dispose();
        }
    }
}