using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace CFOP.Service.Common.DTO
{
    public class User
    {
        public string Skype { get; private set; }
        public IList<string> Aliases { get; private set; }

        public User(string skype) : this(skype, new List<string>())
        {
        }

        [JsonConstructor]
        public User(string skype, IList<string> aliases)
        {
            Skype = skype;
            Aliases = aliases;
        }

        public bool HasAlias(string alias)
        {
            return Aliases.Any(a => a.Equals(alias, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
