using System.Windows;
using Autofac;
using CFOP.AppointmentSchedule;
using CFOP.Common;
using CFOP.External.Calendar.Google;
using CFOP.Infrastructure.Settings;
using CFOP.Service.AppointmentSchedule;
using CFOP.VideoCall;
using Prism.Autofac;
using Prism.Mvvm;
using Prism.Regions;

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

            var regionManager = ComponentContext.Resolve<IRegionManager>();
            regionManager.RegisterViewWithRegion(RegionNames.SideRegion, typeof(AppointmentScheduleView));
            regionManager.RegisterViewWithRegion(RegionNames.TopRegion, typeof(VideoCallView));

            Application.Current.MainWindow = (Window) Shell;
            Application.Current.MainWindow.Show();
        }

        protected override void ConfigureContainerBuilder(ContainerBuilder builder)
        {
            base.ConfigureContainerBuilder(builder);

            builder.RegisterType<MainViewModel>();
            builder.RegisterType<AppointmentScheduleViewModel>();
            builder.RegisterType<VideoCallViewModel>();

            builder.RegisterType<ApplicationSettings>().As<IApplicationSettings>();
            builder.RegisterType<GoogleCalendarService>().As<IManageCalendarService>();

            ViewModelLocationProvider.SetDefaultViewModelFactory(type => Container.Resolve(type));
        }        

        private IComponentContext ComponentContext => Container;
    }
}