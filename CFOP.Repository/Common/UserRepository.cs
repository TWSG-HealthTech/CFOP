using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CFOP.Repository.Data;
using CFOP.Service.Common;
using CFOP.Service.Common.Models;
using Microsoft.EntityFrameworkCore;

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

        public Task<List<User>> FindAllSocialConnections()
        {
            using (var context = new CateContext())
            {
                return context.Users.ToListAsync();
            }
        }
    }
}
