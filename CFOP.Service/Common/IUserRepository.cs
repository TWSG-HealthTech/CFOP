using CFOP.Service.Common.Models;

namespace CFOP.Service.Common
{
    public interface IUserRepository
    {
        User FindById(int id);
        User FindByAlias(string alias);
    }
}
