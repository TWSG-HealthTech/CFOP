using System.Collections.Generic;
using System.IO;
using System.Linq;
using CFOP.Service.Common.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
﻿using System.Collections.Immutable;

namespace CFOP.Repository.Data
{
    public static class Store
    {
        public static void Seed(string usersFilePath)
        {
            using (var ctx = new CateContext())
            {
                var existingMedicines = ctx.Medicines.ToList();
                existingMedicines.ForEach(m => ctx.Medicines.Remove(m));
                ctx.SaveChanges();

                ctx.Medicines.Add(new Medicine() {Name = "Paracetamol"});
                ctx.Medicines.Add(new Medicine() {Name = "Ibuprofen"});
                ctx.Medicines.Add(new Medicine() {Name = "Lemsip"});
                ctx.SaveChanges();

                var users = new List<User>();
                using (var reader = new StreamReader(usersFilePath))
                {
                    users.AddRange(JsonConvert.DeserializeObject<List<User>>(reader.ReadToEnd()));
                }

                var existingUsers = ctx.Users.ToList();
                existingUsers.ForEach(u => ctx.Users.Remove(u));
                ctx.SaveChanges();

                users.ForEach(u => ctx.Users.Add(u));
                ctx.SaveChanges();
            }
        }

        public static IList<Medicine> AllMedicines()
        {
            using (var ctx = new CateContext())
            {
                return ctx.Medicines.ToImmutableList();
            }
        }
    }

    public class CateContext : DbContext
    {
        public CateContext()
        {
            Database.Migrate();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .Ignore(u => u.Aliases)
                .Property(u => u.Id).ValueGeneratedOnAdd();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=./Cate.db");
        }

        public DbSet<Medicine> Medicines { get; set; }
        public DbSet<User> Users { get; set; }
    }

    public class Medicine
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
