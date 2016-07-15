using System.Windows;
using Autofac;
using CFOP.Infrastructure.Settings;
using CFOP.ViewModels;
using CFOP.Views;
using Prism.Autofac;
using Prism.Mvvm;

namespace CFOP
{
    public class Bootstrapper : AutofacBootstrapper
    {
        protected override DependencyObject CreateShell()
        {
            return ComponentContext.Resolve<MainWindow>();
        }

        protected override void InitializeShell()
        {
            base.InitializeShell();

            Application.Current.MainWindow = (Window) Shell;
            Application.Current.MainWindow.Show();
        }

        protected override void ConfigureContainerBuilder(ContainerBuilder builder)
        {
            base.ConfigureContainerBuilder(builder);

            builder.RegisterType<MainViewModel>();
            builder.RegisterType<ApplicationSettings>().As<IApplicationSettings>();

            ViewModelLocationProvider.SetDefaultViewModelFactory(type => Container.Resolve(type));
        }

        private IComponentContext ComponentContext => Container;
    }
}