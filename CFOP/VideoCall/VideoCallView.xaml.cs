using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CFOP.VideoCall
{
    /// <summary>
    /// Interaction logic for VideoCallView.xaml
    /// </summary>
    public partial class VideoCallView : UserControl
    {
        public VideoCallView(VideoCallViewModel viewModel)
        {
            InitializeComponent();

            DataContext = viewModel;
        }
    }
}
