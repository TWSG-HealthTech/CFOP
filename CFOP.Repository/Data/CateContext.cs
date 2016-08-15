using Microsoft.EntityFrameworkCore;

namespace CFOP.Repository.Data
{
    public static class Store
    {
        public static void Seed()
        {
            using (var ctx = new CateContext())
            {
                ctx.Medicines.Add(new Medicine() {Name = "Paracetamol"});
                ctx.Medicines.Add(new Medicine() {Name = "Ibuprofen"});
                ctx.Medicines.Add(new Medicine() {Name = "Lemsip"});
                ctx.SaveChanges();
            }
        }
    }

    public class CateContext : DbContext
    {
        public CateContext()
        {
            Database.Migrate();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=./Cate.db");
        }

        public DbSet<Medicine> Medicines { get; set; }
    }

    public class Medicine
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
