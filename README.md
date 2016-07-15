### To integrate with Skype, open Powershell/Cmd with admin priviledge, cd to Libs folder and run:
```
regsvr32.exe Skype4COM.dll
```

### To integrate with Google calendar

- Follow instruction at https://developers.google.com/google-apps/calendar/quickstart/dotnet to create client secret json file
- Download client secret file and rename to client_secret_{user_name}.json and move to Secrets/ sub folder in CFOP.External.Calendar.Google project
- Change "Copy to Output Directory" to always