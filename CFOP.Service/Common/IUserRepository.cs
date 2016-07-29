using CFOP.Service.Common.DTO;

namespace CFOP.Service.Common
{
    public interface IUserRepository
    {
        User FindById(int id);
        User FindByAlias(string alias);
    }
}
