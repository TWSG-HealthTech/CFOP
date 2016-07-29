using CFOP.Service.Common.DTO;

namespace CFOP.Service.Common
{
    public class ManageUserService : IManageUserService
    {
        private readonly IUserRepository _userRepository;

        public ManageUserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public User LookUpUserByAlias(string alias)
        {
            return _userRepository.FindByAlias(alias);
        }
    }
}
