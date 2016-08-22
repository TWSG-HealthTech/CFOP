using System.Windows;
using CFOP.Repository.Scheduler;

namespace CFOP
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            Initialiser.init();
            var bootstrapper = new Bootstrapper();
            bootstrapper.Run();
        }
    }
}
