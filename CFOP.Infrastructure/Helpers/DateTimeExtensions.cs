using System;

namespace CFOP.Infrastructure.Helpers
{
    public static class DateTimeExtensions
    {
        public static DateTime ToDate(this DateTime time)
        {
            return new DateTime(time.Year, time.Month, time.Day);
        }
    }
}
