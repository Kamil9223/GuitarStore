﻿// <auto-generated />
using System;
using Customers.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Customers.Infrastructure.Migrations
{
    [DbContext(typeof(CustomersDbContext))]
    [Migration("20230914165052_ChangeRelationCustomerCart")]
    partial class ChangeRelationCustomerCart
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.2")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder, 1L, 1);

            modelBuilder.Entity("Customers.Domain.Carts.Cart", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2")
                        .HasColumnName("CreatedAt");

                    b.HasKey("Id");

                    b.ToTable("Carts", "Customers");
                });

            modelBuilder.Entity("Customers.Domain.Customers.Customer", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<int?>("CartId")
                        .HasColumnType("int");

                    b.Property<string>("LastName")
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("Name")
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.HasKey("Id");

                    b.HasIndex("CartId");

                    b.ToTable("Customers", "Customers");
                });

            modelBuilder.Entity("Customers.Domain.Products.Product", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<string>("Name")
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.Property<int>("Quantity")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("Name")
                        .IsUnique()
                        .HasFilter("[Name] IS NOT NULL");

                    b.ToTable("Products", "Customers");
                });

            modelBuilder.Entity("Customers.Domain.Carts.Cart", b =>
                {
                    b.OwnsMany("Customers.Domain.Carts.CartItem", "CartItems", b1 =>
                        {
                            b1.Property<int>("Id")
                                .ValueGeneratedOnAdd()
                                .HasColumnType("int");

                            SqlServerPropertyBuilderExtensions.UseIdentityColumn(b1.Property<int>("Id"), 1L, 1);

                            b1.Property<int>("CartId")
                                .HasColumnType("int");

                            b1.Property<string>("Name")
                                .HasMaxLength(200)
                                .HasColumnType("nvarchar(200)")
                                .HasColumnName("Name");

                            b1.Property<int>("ProductId")
                                .HasColumnType("int")
                                .HasColumnName("ProductId");

                            b1.Property<int>("Quantity")
                                .HasColumnType("int")
                                .HasColumnName("Quantity");

                            b1.HasKey("Id");

                            b1.HasIndex("CartId");

                            b1.ToTable("CartItems", "Customers");

                            b1.WithOwner()
                                .HasForeignKey("CartId");

                            b1.OwnsOne("Domain.ValueObjects.Money", "Price", b2 =>
                                {
                                    b2.Property<int>("CartItemId")
                                        .HasColumnType("int");

                                    b2.Property<decimal>("Value")
                                        .HasColumnType("decimal(10,2)")
                                        .HasColumnName("Price");

                                    b2.HasKey("CartItemId");

                                    b2.ToTable("CartItems", "Customers");

                                    b2.WithOwner()
                                        .HasForeignKey("CartItemId");
                                });

                            b1.Navigation("Price");
                        });

                    b.Navigation("CartItems");
                });

            modelBuilder.Entity("Customers.Domain.Customers.Customer", b =>
                {
                    b.HasOne("Customers.Domain.Carts.Cart", "Cart")
                        .WithMany()
                        .HasForeignKey("CartId");

                    b.OwnsOne("Customers.Domain.Customers.CustomerAddress", "Address", b1 =>
                        {
                            b1.Property<int>("CustomerId")
                                .HasColumnType("int");

                            b1.Property<string>("Country")
                                .HasMaxLength(300)
                                .HasColumnType("nvarchar(300)")
                                .HasColumnName("Country");

                            b1.Property<string>("HouseNumber")
                                .HasMaxLength(20)
                                .HasColumnType("nvarchar(20)")
                                .HasColumnName("HouseNumber");

                            b1.Property<string>("LocalNumber")
                                .HasMaxLength(20)
                                .HasColumnType("nvarchar(20)")
                                .HasColumnName("LocalNumber");

                            b1.Property<string>("Locality")
                                .HasMaxLength(15)
                                .HasColumnType("nvarchar(15)")
                                .HasColumnName("Locality");

                            b1.Property<string>("LocalityName")
                                .HasMaxLength(500)
                                .HasColumnType("nvarchar(500)")
                                .HasColumnName("LocalityName");

                            b1.Property<string>("PostalCode")
                                .HasMaxLength(10)
                                .HasColumnType("nvarchar(10)")
                                .HasColumnName("PostalCode");

                            b1.Property<string>("Street")
                                .HasMaxLength(500)
                                .HasColumnType("nvarchar(500)")
                                .HasColumnName("Street");

                            b1.HasKey("CustomerId");

                            b1.ToTable("Customers", "Customers");

                            b1.WithOwner()
                                .HasForeignKey("CustomerId");
                        });

                    b.OwnsOne("Domain.ValueObjects.EmailAddress", "Email", b1 =>
                        {
                            b1.Property<int>("CustomerId")
                                .HasColumnType("int");

                            b1.Property<string>("Email")
                                .IsRequired()
                                .HasMaxLength(100)
                                .HasColumnType("nvarchar(100)")
                                .HasColumnName("Email");

                            b1.HasKey("CustomerId");

                            b1.ToTable("Customers", "Customers");

                            b1.WithOwner()
                                .HasForeignKey("CustomerId");
                        });

                    b.Navigation("Address");

                    b.Navigation("Cart");

                    b.Navigation("Email");
                });

            modelBuilder.Entity("Customers.Domain.Products.Product", b =>
                {
                    b.OwnsOne("Domain.ValueObjects.Money", "Price", b1 =>
                        {
                            b1.Property<int>("ProductId")
                                .HasColumnType("int");

                            b1.Property<decimal>("Value")
                                .HasColumnType("decimal(10,2)")
                                .HasColumnName("Price");

                            b1.HasKey("ProductId");

                            b1.ToTable("Products", "Customers");

                            b1.WithOwner()
                                .HasForeignKey("ProductId");
                        });

                    b.Navigation("Price");
                });
#pragma warning restore 612, 618
        }
    }
}
