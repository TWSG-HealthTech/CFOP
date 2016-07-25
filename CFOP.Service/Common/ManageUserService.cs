using CFOP.Infrastructure.Settings;
using CFOP.Service.Common.DTO;

namespace CFOP.Service.Common
{
    public class ManageUserService : IManageUserService
    {
        private readonly FileBasedUserRegistry _userRegistry;

        public ManageUserService(IApplicationSettings applicationSettings)
        {
            _userRegistry = new FileBasedUserRegistry(applicationSettings.UsersFilePath);
        }

        public User LookUpUserByAlias(string alias)
        {
            return _userRegistry.LookUpByAlias(alias);
        }
    }
}
