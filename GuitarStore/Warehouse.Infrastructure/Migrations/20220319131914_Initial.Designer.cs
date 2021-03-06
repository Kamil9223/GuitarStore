// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Warehouse.Infrastructure.Database;

#nullable disable

namespace Warehouse.Infrastructure.Migrations
{
    [DbContext(typeof(WarehouseDbContext))]
    [Migration("20220319131914_Initial")]
    partial class Initial
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.2")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder, 1L, 1);

            modelBuilder.Entity("ElectricGuitarPickup", b =>
                {
                    b.Property<int>("ElectricGuitarsId")
                        .HasColumnType("int");

                    b.Property<int>("PickupsId")
                        .HasColumnType("int");

                    b.HasKey("ElectricGuitarsId", "PickupsId");

                    b.HasIndex("PickupsId");

                    b.ToTable("ElectricGuitarPickup", "Warehouse");
                });

            modelBuilder.Entity("Warehouse.Domain.AcousticGuitars.AcousticGuitar", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<string>("CompanyName")
                        .IsRequired()
                        .HasMaxLength(75)
                        .HasColumnType("nvarchar(75)");

                    b.Property<int>("GuitarStoreId")
                        .HasColumnType("int");

                    b.Property<string>("ModelName")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<decimal>("Price")
                        .HasColumnType("decimal(10,2)");

                    b.HasKey("Id");

                    b.HasIndex("GuitarStoreId");

                    b.ToTable("AcousticGuitars", "Warehouse");
                });

            modelBuilder.Entity("Warehouse.Domain.ElectricGuitars.ElectricGuitar", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<string>("CompanyName")
                        .IsRequired()
                        .HasMaxLength(75)
                        .HasColumnType("nvarchar(75)");

                    b.Property<int>("GuitarStoreId")
                        .HasColumnType("int");

                    b.Property<string>("ModelName")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<decimal>("Price")
                        .HasColumnType("decimal(10,2)");

                    b.HasKey("Id");

                    b.HasIndex("GuitarStoreId");

                    b.ToTable("ElectricGuitars", "Warehouse");
                });

            modelBuilder.Entity("Warehouse.Domain.ElectricGuitars.Pickup", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("PickupType")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("nvarchar(30)");

                    b.HasKey("Id");

                    b.ToTable("Pickups", "Warehouse");
                });

            modelBuilder.Entity("Warehouse.Domain.Store.GuitarStore", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("nvarchar(30)");

                    b.HasKey("Id");

                    b.ToTable("Stores", "Warehouse");
                });

            modelBuilder.Entity("ElectricGuitarPickup", b =>
                {
                    b.HasOne("Warehouse.Domain.ElectricGuitars.ElectricGuitar", null)
                        .WithMany()
                        .HasForeignKey("ElectricGuitarsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Warehouse.Domain.ElectricGuitars.Pickup", null)
                        .WithMany()
                        .HasForeignKey("PickupsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Warehouse.Domain.AcousticGuitars.AcousticGuitar", b =>
                {
                    b.HasOne("Warehouse.Domain.Store.GuitarStore", "GuitarStore")
                        .WithMany("AcousticGuitars")
                        .HasForeignKey("GuitarStoreId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("GuitarStore");
                });

            modelBuilder.Entity("Warehouse.Domain.ElectricGuitars.ElectricGuitar", b =>
                {
                    b.HasOne("Warehouse.Domain.Store.GuitarStore", "GuitarStore")
                        .WithMany("ElectricGuitars")
                        .HasForeignKey("GuitarStoreId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("GuitarStore");
                });

            modelBuilder.Entity("Warehouse.Domain.Store.GuitarStore", b =>
                {
                    b.OwnsOne("Warehouse.Domain.Store.StoreLocation", "Location", b1 =>
                        {
                            b1.Property<int>("GuitarStoreId")
                                .HasColumnType("int");

                            b1.Property<string>("City")
                                .IsRequired()
                                .HasMaxLength(200)
                                .HasColumnType("nvarchar(200)")
                                .HasColumnName("City");

                            b1.Property<string>("PostalCode")
                                .IsRequired()
                                .HasColumnType("char(6)")
                                .HasColumnName("PostalCode");

                            b1.Property<string>("Street")
                                .IsRequired()
                                .HasMaxLength(400)
                                .HasColumnType("nvarchar(400)")
                                .HasColumnName("Street");

                            b1.HasKey("GuitarStoreId");

                            b1.ToTable("Stores", "Warehouse");

                            b1.WithOwner()
                                .HasForeignKey("GuitarStoreId");
                        });

                    b.Navigation("Location")
                        .IsRequired();
                });

            modelBuilder.Entity("Warehouse.Domain.Store.GuitarStore", b =>
                {
                    b.Navigation("AcousticGuitars");

                    b.Navigation("ElectricGuitars");
                });
#pragma warning restore 612, 618
        }
    }
}
