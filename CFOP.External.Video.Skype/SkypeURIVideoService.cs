using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows.Automation;
using CFOP.Service.Common.Models;
using CFOP.Service.VideoCall;

namespace CFOP.External.Video.Skype
{
    public class SkypeURIVideoService : IVideoService
    {
        public void Call(User user, Action finishCallback)
        {
            Process.Start($"skype:{user.Skype}?call&video=true");

            WaitForCallToFinishAndSwitchBack(finishCallback);
        }

        private static void WaitForCallToFinishAndSwitchBack(Action finishCallback)
        {
            var skypeProcess =
                Process.GetProcessesByName("Skype").FirstOrDefault(s => !string.IsNullOrWhiteSpace(s.MainWindowTitle));
            if (skypeProcess == null) return;

            var root = AutomationElement.RootElement;
            if (root == null) return;

            var thisApplication = Find(() => root.FindFirst(TreeScope.Children,
                new PropertyCondition(AutomationElement.NameProperty, "CFOP")));
            
            var skypeConversationForm = FindInDescendant(root,
                new PropertyCondition(AutomationElement.ProcessIdProperty, skypeProcess.Id),
                new PropertyCondition(AutomationElement.ClassNameProperty, "TConversationForm"));
            if (skypeConversationForm == null) return;

            //Wait for events when child elements of this conversation form are added/removed
            Automation.AddStructureChangedEventHandler(skypeConversationForm, TreeScope.Children, (s, e) => OnStructureChanged(s, e, thisApplication, finishCallback));
        }

        private static void OnStructureChanged(object sender, StructureChangedEventArgs arg, AutomationElement thisAppplication, Action finishCallback)
        {
            //If element with class name TChatContentControl is added to conversation form, 
            //it means the video call is finished and it's back to normal chat interface, 
            //so switch back to CFOP application
            var element = sender as AutomationElement;
            if (arg.StructureChangeType == StructureChangeType.ChildAdded && 
                element != null && 
                element.Current.ClassName == "TChatContentControl")
            {
                thisAppplication.SetFocus();
                finishCallback();
            }
        }

        private static AutomationElement FindInDescendant(AutomationElement root, params PropertyCondition[] conditions)
        {
            var current = root;
            foreach (var condition in conditions)
            {
                var foundElement = Find(() => current.FindFirst(TreeScope.Children, condition));
                if (foundElement == null) return null;

                current = foundElement;
            }

            return current;
        }

        private static AutomationElement Find(Func<AutomationElement> action, int maxLoop = 10)
        {
            var element = action();
            var currentCount = 0;
            while (element == null && currentCount < maxLoop)
            {
                element = action();

                currentCount++;
                Thread.Sleep(100);
            }

            return element;
        }

        private static List<AutomationElement> FindAllChildren(AutomationElement parent)
        {
            return new List<AutomationElement>(
                        parent.FindAll(TreeScope.Children, Condition.TrueCondition).Cast<AutomationElement>());
        }

        private static List<AutomationElement> FindAllDescendants(AutomationElement parent)
        {
            return new List<AutomationElement>(
                        parent.FindAll(TreeScope.Descendants, Condition.TrueCondition).Cast<AutomationElement>());
        }
    }
}
