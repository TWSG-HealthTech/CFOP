using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using CFOP.Repository.Data;

namespace CFOP.Repository.Migrations
{
    [DbContext(typeof(CateContext))]
    [Migration("20160817092355_MedicationSchedules")]
    partial class MedicationSchedules
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.0.0-rtm-21431");

            modelBuilder.Entity("CFOP.Repository.Data.Course", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int?>("MedicineId");

                    b.HasKey("Id");

                    b.HasIndex("MedicineId");

                    b.ToTable("Course");
                });

            modelBuilder.Entity("CFOP.Repository.Data.Medicine", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name");

                    b.Property<string>("Unit");

                    b.HasKey("Id");

                    b.ToTable("Medicines");
                });

            modelBuilder.Entity("CFOP.Repository.Data.Schedule", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int?>("CourseId");

                    b.Property<int>("DayOfWeek");

                    b.Property<string>("Notes");

                    b.Property<int>("Quantity");

                    b.Property<TimeSpan>("Time");

                    b.HasKey("Id");

                    b.HasIndex("CourseId");

                    b.ToTable("MedicationSchedules");
                });

            modelBuilder.Entity("CFOP.Repository.Data.SocialClub", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ClubName");

                    b.Property<string>("ContactNumber");

                    b.Property<string>("Venue");

                    b.HasKey("Id");

                    b.ToTable("SocialClubs");
                });

            modelBuilder.Entity("CFOP.Service.Common.Models.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("CalendarClientSecret");

                    b.Property<string>("CalendarEmail");

                    b.Property<string>("CalendarNames");

                    b.Property<string>("SerializedAliases");

                    b.Property<string>("Skype");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("CFOP.Repository.Data.Course", b =>
                {
                    b.HasOne("CFOP.Repository.Data.Medicine", "Medicine")
                        .WithMany("Courses")
                        .HasForeignKey("MedicineId");
                });

            modelBuilder.Entity("CFOP.Repository.Data.Schedule", b =>
                {
                    b.HasOne("CFOP.Repository.Data.Course", "Course")
                        .WithMany("Schedules")
                        .HasForeignKey("CourseId");
                });
        }
    }
}
