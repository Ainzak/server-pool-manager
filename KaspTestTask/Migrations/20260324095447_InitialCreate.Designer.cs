using System;
using KaspTestTask.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace KaspTestTask.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260324095447_InitialCreate")]
    partial class InitialCreate
    {
        /        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.0");

            modelBuilder.Entity("KaspTestTask.Models.Server", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<int>("CpuCores")
                        .HasColumnType("INTEGER");

                    b.Property<int>("DiskGb")
                        .HasColumnType("INTEGER");

                    b.Property<string>("OperatingSystem")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("RamMb")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime?>("ReadyAt")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("ReservedUntil")
                        .HasColumnType("TEXT");

                    b.Property<byte[]>("RowVersion")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("BLOB");

                    b.Property<int>("Status")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("Servers");
                });
#pragma warning restore 612, 618
        }
    }
}
