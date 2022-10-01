﻿// <auto-generated />
using System;
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
    [Migration("20220930170431_ChangeWarehouseModel")]
    partial class ChangeWarehouseModel
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.2")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder, 1L, 1);

            modelBuilder.Entity("Warehouse.Domain.Product.Category", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<string>("CategoryName")
                        .IsRequired()
                        .HasMaxLength(75)
                        .HasColumnType("nvarchar(75)");

                    b.Property<int?>("ParentCategoryId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("ParentCategoryId");

                    b.ToTable("Categories", "Warehouse");
                });

            modelBuilder.Entity("Warehouse.Domain.Product.Product", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<int>("CategoryId")
                        .HasColumnType("int");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("GuitarStoreId")
                        .HasColumnType("int");

                    b.Property<string>("ModelName")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<decimal>("Price")
                        .HasColumnType("decimal(10,2)");

                    b.Property<string>("ProducerName")
                        .IsRequired()
                        .HasMaxLength(75)
                        .HasColumnType("nvarchar(75)");

                    b.HasKey("Id");

                    b.HasIndex("CategoryId");

                    b.HasIndex("GuitarStoreId");

                    b.HasIndex("ProducerName")
                        .IsUnique();

                    b.ToTable("Products", "Warehouse");
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

            modelBuilder.Entity("Warehouse.Domain.Product.Category", b =>
                {
                    b.HasOne("Warehouse.Domain.Product.Category", "ParentCategory")
                        .WithMany("SubCategories")
                        .HasForeignKey("ParentCategoryId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.Navigation("ParentCategory");
                });

            modelBuilder.Entity("Warehouse.Domain.Product.Product", b =>
                {
                    b.HasOne("Warehouse.Domain.Product.Category", "Category")
                        .WithMany("Products")
                        .HasForeignKey("CategoryId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Warehouse.Domain.Store.GuitarStore", "GuitarStore")
                        .WithMany("Products")
                        .HasForeignKey("GuitarStoreId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Category");

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

            modelBuilder.Entity("Warehouse.Domain.Product.Category", b =>
                {
                    b.Navigation("Products");

                    b.Navigation("SubCategories");
                });

            modelBuilder.Entity("Warehouse.Domain.Store.GuitarStore", b =>
                {
                    b.Navigation("Products");
                });
#pragma warning restore 612, 618
        }
    }
}
