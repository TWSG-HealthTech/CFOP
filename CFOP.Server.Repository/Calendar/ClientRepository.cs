using System.Collections.Generic;
using System.Linq;
using CFOP.Server.Core.Calendar;
using Microsoft.EntityFrameworkCore;

namespace CFOP.Server.Repository.Calendar
{
    public class ClientRepository : IClientRepository
    {
        private readonly ServerContext _context;
        private readonly DbSet<Client> _set;

        public ClientRepository(ServerContext context)
        {
            _context = context;
            _set = _context.Set<Client>();
        }

        public List<Client> FindAll()
        {
            return _set.ToList();
        }

        public void Add(Client client)
        {
            _set.Add(client);

            _context.SaveChanges();
        }

        public void DeleteBy(string connectionId)
        {
            var toBeDeleted = _set.FirstOrDefault(c => c.ConnectionId == connectionId);

            if (toBeDeleted == null) return;

            _set.Remove(toBeDeleted);
            _context.SaveChanges();
        }
    }
}
