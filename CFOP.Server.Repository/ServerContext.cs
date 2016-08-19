using CFOP.Server.Core.Calendar;
using Microsoft.EntityFrameworkCore;

namespace CFOP.Server.Repository
{
    public class ServerContext : DbContext
    {
        public ServerContext(DbContextOptions<ServerContext> options)
            : base(options)
        {
            
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Client>().HasKey(c => c.ConnectionId);
        }
    }
}
