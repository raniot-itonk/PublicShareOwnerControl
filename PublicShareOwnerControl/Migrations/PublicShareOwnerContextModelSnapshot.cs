﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PublicShareOwnerControl.DB;

namespace PublicShareOwnerControl.Migrations
{
    [DbContext(typeof(PublicShareOwnerContext))]
    partial class PublicShareOwnerContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.2.0-rtm-35687")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("PublicShareOwnerControl.DB.Shareholder", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("Amount");

                    b.Property<long?>("StockId");

                    b.HasKey("Id");

                    b.HasIndex("StockId");

                    b.ToTable("Shareholder");
                });

            modelBuilder.Entity("PublicShareOwnerControl.DB.Stock", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<double>("LastTradedValue");

                    b.Property<string>("Name");

                    b.HasKey("Id");

                    b.ToTable("Stocks");
                });

            modelBuilder.Entity("PublicShareOwnerControl.DB.Shareholder", b =>
                {
                    b.HasOne("PublicShareOwnerControl.DB.Stock")
                        .WithMany("ShareHolders")
                        .HasForeignKey("StockId");
                });
#pragma warning restore 612, 618
        }
    }
}
