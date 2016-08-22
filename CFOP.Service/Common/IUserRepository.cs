using System.Collections.Generic;
using System.Threading.Tasks;
using CFOP.Service.Common.Models;

namespace CFOP.Service.Common
{
    public interface IUserRepository
    {
        User FindById(int id);
        User FindByAlias(string alias);
        Task<List<User>> FindAllSocialConnections();
    }
}
