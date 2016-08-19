using CFOP.Server.Core.Calendar;

namespace CFOP.Server.Repository
{
    public static class DbInitializer
    {
        public static void Seed(ServerContext context)
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
        }
    }
}
