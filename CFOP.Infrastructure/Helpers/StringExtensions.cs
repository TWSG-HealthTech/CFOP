using System.Collections.Generic;
using System.Linq;

namespace CFOP.Infrastructure.Helpers
{
    public static class StringExtensions
    {
        public static List<string> CSVToList(this string input)
        {
            return input.Split(',').ToList();
        }
    }
}
