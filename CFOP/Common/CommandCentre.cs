using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Commands;

namespace CFOP.Common
{
    public static class CommandCentre
    {
        public static event Action<DateTime> ShowCalendarCommand;

        public static void ShowCalendar(DateTime day)
        {
            ShowCalendarCommand(day);
        }
    }
}
