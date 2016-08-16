using System;
using System.Collections.Generic;
using System.Linq;
using CFOP.Infrastructure.Helpers;
using CFOP.Infrastructure.JSON;
using Newtonsoft.Json;

namespace CFOP.Service.Common.Models
{
    public class User
    {
        public int Id { get; private set; }
        public string Skype { get; private set; }
        public IList<string> Aliases { get; private set; }
        public string CalendarEmail { get; private set; }
        public string CalendarNames { get; private set; }

        [JsonConverter(typeof(JsonObjectAsStringConverter))]
        public string CalendarClientSecret { get; private set; }

        [JsonIgnore]
        public string SerializedAliases
        {
            get { return string.Join(",", Aliases); }
            set { Aliases = value.CSVToList(); }
        }

        [JsonConstructor]
        public User(int id, 
                    string skype, 
                    IList<string> aliases, 
                    string calendarEmail, 
                    string calendarNames, 
                    string calendarClientSecret)
        {
            Id = id;
            Skype = skype;
            Aliases = aliases;
            CalendarEmail = calendarEmail;
            CalendarNames = calendarNames;
            CalendarClientSecret = calendarClientSecret;
        }

        // For EF
        private User() { }

        public bool HasAlias(string alias)
        {
            return Aliases.Any(a => a.Equals(alias, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
