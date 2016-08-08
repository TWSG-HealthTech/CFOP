using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows.Automation;
using CFOP.Service.Common.DTO;
using CFOP.Service.VideoCall;

namespace CFOP.External.Video.Skype
{
    public class SkypeURIVideoService : IVideoService
    {
        public void Call(User user)
        {
            Process.Start($"skype:{user.Skype}?call&video=true");

            WaitForCallToFinishAndSwitchBack();
        }

        private void WaitForCallToFinishAndSwitchBack()
        {
            var skypeProcess =
                Process.GetProcessesByName("Skype").FirstOrDefault(s => !string.IsNullOrWhiteSpace(s.MainWindowTitle));
            if (skypeProcess == null) return;

            var root = AutomationElement.RootElement;
            if (root == null) return;

            var thisApplication = Find(() => root.FindFirst(TreeScope.Children,
                new PropertyCondition(AutomationElement.NameProperty, "CFOP")));
            var skypeApplication = Find(() => root.FindFirst(TreeScope.Children,
                new PropertyCondition(AutomationElement.ProcessIdProperty, skypeProcess.Id)));
            if (skypeApplication == null) return;

            var videoCallMainWindowCondition = new PropertyCondition(AutomationElement.ClassNameProperty,
                "TConversationForm");
            var videoCallMainWindow = Find(() => skypeApplication.FindFirst(TreeScope.Children, videoCallMainWindowCondition));
            if (videoCallMainWindow == null) return;

            var videoCallWindowLocation = videoCallMainWindow.Current.BoundingRectangle;
            do
            {
                Thread.Sleep(1000);
                videoCallMainWindow = skypeApplication.FindFirst(TreeScope.Children, videoCallMainWindowCondition);
            } while (videoCallMainWindow.Current.BoundingRectangle == videoCallWindowLocation);

            thisApplication.SetFocus();
        }

        private static AutomationElement Find(Func<AutomationElement> action, int maxLoop = 10)
        {
            var element = action();
            var currentCount = 0;
            while (element == null && currentCount < maxLoop)
            {
                element = action();
            }

            return element;
        }
    }
}
