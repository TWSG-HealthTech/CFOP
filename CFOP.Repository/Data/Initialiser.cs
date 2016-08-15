using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CFOP.Repository.Data
{
    public class Initialiser
    {
        public static void Init()
        {
//            Database.SetInitializer(new DropCreateDatabaseIfModelChanges<CateContext>());
            var context = new CateContext();
            context.Medicines.Add(new Medicine {Name = "Paracetamol"});
            context.SaveChanges();
        }
    }

    public class CateContext : DbContext
    {
        public DbSet<Medicine> Medicines { get; set; }
    }

    public class Medicine
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
