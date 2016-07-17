### Tech Stack
```
- WPF
- Prism
- Autofac
```

### To integrate with Skype, open Powershell/Cmd with admin priviledge, cd to Libs folder and run:
```
regsvr32.exe Skype4COM.dll
```

### To integrate with Google calendar

- Follow instruction at https://developers.google.com/google-apps/calendar/quickstart/dotnet to create client secret json file
- Download client secret file and rename to client_secret_{user_name}.json and move to Secrets/ sub folder in `CFOP.External.Calendar.Google` project
- Change "Copy to Output Directory" to always

### Code Structure

- The project uses Prism region to separate view into different UserControls. Once the application is started, Prism will load UserControl into region in MainWindow according to mapping in Bootstrapper class, .e.g.:
```
regionManager.RegisterViewWithRegion(RegionNames.TopRegion, typeof(VideoCallView));
```

- ViewModel is injected into View by constructor injection and need to be assigned manually to View `DataContext` for data binding. Autofac container is configured in `Bootstrapper.ConfigureContainerBuilder()`

### Speech Recognition

- To get speech recognition to work, you will need to add in your keys to the AppSettings.secret.config file.  The file should look similar to the following:
```
<?xml version="1.0" encoding="utf-8" ?>
<appSettings>
  <add key="primaryKey" value="???"/>
  <add key="secondaryKey" value="???"/>
  <add key="luisAppId" value="???"/>
  <add key="luisSubscriptionId" value="???"/>
</appSettings>
```

- Download and install Microsoft Speech Runtime 11 http://go.microsoft.com/fwlink/?LinkID=223568&clcid=0x409
- Download and install one or many Runtime Languages: https://www.microsoft.com/en-us/download/details.aspx?id=27224
    (One language runtime includes one SR and one TTS file)
- Open regedit, navigate to `HKEY_CURRENT_USER\Software\Microsoft\Speech`, right click `CurrentUserLexicon` choose `Permissions`. If there is an entry for `ALL APPLICATION PACKAGES`, remove it