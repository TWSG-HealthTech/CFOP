using System;
using System.Collections.Generic;
using System.IO;
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
                ctx.Medicines.RemoveRange(ctx.Medicines);
                ctx.SaveChanges();

                ctx.Medicines.Add(new Medicine{Name = "Paracetamol", Unit = "Capsule"});
                ctx.Medicines.Add(new Medicine{Name = "Ibuprofen", Unit = "Tablet"});
                var lemsip = new Medicine {Name = "Lemsip", Unit = "Sachet"};
                ctx.Medicines.Add(lemsip);
                
                var course = new Course {Medicine = lemsip};
                foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek)))
                {
                    foreach (var time in new []{ 9, 13, 19})
                    {
                        ctx.MedicationSchedules.Add(new Schedule { Course = course, DayOfWeek = day, Time = new TimeSpan(time, 0, 0), Quantity = 1, Notes = "After food"});
                    }
                }
                ctx.SaveChanges();


                var users = new List<User>();
                using (var reader = new StreamReader(usersFilePath))
                {
                    users.AddRange(JsonConvert.DeserializeObject<List<User>>(reader.ReadToEnd()));
                }

                ctx.Users.RemoveRange(ctx.Users);
                ctx.SaveChanges();

                users.ForEach(u => ctx.Users.Add(u));
                ctx.SaveChanges();

                ctx.SocialClubs.RemoveRange(ctx.SocialClubs);
                ctx.SaveChanges();

                ctx.SocialClubs.Add(new SocialClub() { ClubName = "Rambling", Venue = "Amoy Street", ContactNumber = "65319827" });
                ctx.SocialClubs.Add(new SocialClub() { ClubName = "Tea time club", Venue = "Telok Ayer Street", ContactNumber = "91234567" });
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

        public static IList<SocialClub> AllSocialClubs()
        {
            using (var ctx = new CateContext())
            {
                return ctx.SocialClubs.ToImmutableList();
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
        public DbSet<SocialClub> SocialClubs { get; set; }
        public DbSet<Schedule> MedicationSchedules { get; set; }
    }

    public class Medicine
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Unit { get; set; }
        public IList<Course> Courses { get; set; }
    }

    public class Course
    {
        public int Id { get; set; }
        public Medicine Medicine { get; set; }
        public List<Schedule> Schedules { get; set; }
    }

    public class Schedule
    {
        public int Id { get; set; }
        public Course Course { get; set; }
        public int Quantity { get; set; }
        public TimeSpan Time { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public string Notes { get; set; }
    }

    public class SocialClub
    {
        public int Id { get; set; }
        public string ClubName { get; set; }
        public string Venue { get; set; }
        public string ContactNumber { get; set; }
    }
}
