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
            modelBuilder.Entity<Subscription>(builder =>
            {
                builder.HasKey(s => s.Id);
                builder.Property(s => s.Id)
                    .ValueGeneratedOnAdd();
            });

            modelBuilder.Entity<Client>(builder =>
            {
                builder.HasKey(c => c.Id);
                builder.Property(c => c.Id)
                    .ValueGeneratedOnAdd();
                builder.HasMany(c => c.Subscriptions)
                    .WithOne();
            });

        }
    }
}
