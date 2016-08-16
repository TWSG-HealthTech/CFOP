using System.Collections.Generic;
using System.IO;
using System.Linq;
using CFOP.Infrastructure.JSON;
using CFOP.Infrastructure.Settings;
using CFOP.Service.Common;
using CFOP.Service.Common.Models;
using Newtonsoft.Json;

namespace CFOP.Repository.Common
{
    public class FileBasedUserRepository : IUserRepository
    {
        private readonly string _filePath;
        private IList<User> _users;

        public FileBasedUserRepository(IApplicationSettings applicationSettings)
        {
            _filePath = applicationSettings.UsersFilePath;
        }

        public User FindById(int id)
        {
            if (_users == null)
            {
                _users = ReadUserJsonFile();
            }

            return _users.FirstOrDefault(u => u.Id == id);
        }

        public User FindByAlias(string alias)
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
