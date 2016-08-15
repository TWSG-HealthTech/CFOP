using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CFOP.Repository.Data;

namespace CFOP
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            Initialiser.Init();
            var bootstrapper = new Bootstrapper();
            bootstrapper.Run();
        }
    }
}
