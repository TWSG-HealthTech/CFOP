using System.Linq;
using CFOP.Repository.Data;
using CFOP.Service.Common;
using CFOP.Service.Common.Models;

namespace CFOP.Repository.Common
{
    public class UserRepository : IUserRepository
    {
        public User FindById(int id)
        {
            using (var context = new CateContext())
            {
                return context.Users.FirstOrDefault(u => u.Id == id);
            }
        }

        public User FindByAlias(string alias)
        {
            using (var context = new CateContext())
            {
                return context.Users.FirstOrDefault(u => u.Aliases.Any(a => a == alias));
            }
        }
    }
}
