using CFOP.Service.Common.DTO;

namespace CFOP.Service.Common
{
    public interface IManageUserService
    {
        User LookUpUserByAlias(string alias);
    }
}
