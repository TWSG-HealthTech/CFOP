using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CFOP.Infrastructure.JSON;
using Newtonsoft.Json;

namespace CFOP.Service.Common.DTO
{
    public class User
    {
        public class CalendarSettings
        {
            public class GoogleCalendarSettings
            {
                public string Email { get; set; }
                [JsonConverter(typeof(CsvToListPropertyConverter))]
                public List<string> CalendarNames { get; set; }

                [JsonConverter(typeof(JsonObjectAsStringConverter))]
                public string ClientSecret { get; set; }
            }

            public GoogleCalendarSettings Google { get; set; }
        }

        public int Id { get; private set; }
        public string Skype { get; private set; }
        public IList<string> Aliases { get; private set; }
        public CalendarSettings Calendar { get; private set; }

        [JsonConstructor]
        public User(int id, string skype, IList<string> aliases, CalendarSettings calendar)
        {
            Id = id;
            Skype = skype;
            Aliases = aliases;
            Calendar = calendar;
        }

        public bool HasAlias(string alias)
        {
            return Aliases.Any(a => a.Equals(alias, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
