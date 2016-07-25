using System.Collections.Generic;
using System.IO;
using System.Linq;
using CFOP.Service.Common.DTO;
using Newtonsoft.Json;

namespace CFOP.Service.Common
{
    public class FileBasedUserRegistry
    {
        private readonly string _filePath;
        private IList<User> _users;

        public FileBasedUserRegistry(string filePath)
        {
            _filePath = filePath;
        }

        public User LookUpByAlias(string alias)
        {
            if (_users == null)
            {
                _users = ReadUserJsonFile();
            }

            return _users.FirstOrDefault(u => u.HasAlias(alias));
        }

        private IList<User> ReadUserJsonFile()
        {
            using (var reader = new StreamReader(_filePath))
            {
                return JsonConvert.DeserializeObject<List<User>>(reader.ReadToEnd());
            }
        }
    }
}
