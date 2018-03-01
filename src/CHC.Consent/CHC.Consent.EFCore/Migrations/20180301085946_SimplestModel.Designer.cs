﻿// <auto-generated />
using CHC.Consent.EFCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using System;

namespace CHC.Consent.EFCore.Migrations
{
    [DbContext(typeof(ConsentContext))]
    [Migration("20180301085946_SimplestModel")]
    partial class SimplestModel
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.0.1-rtm-125")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("CHC.Consent.EFCore.Entities.PersonEntity", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.HasKey("Id");

                    b.ToTable("Person");
                });

            modelBuilder.Entity("CHC.Consent.EFCore.Entities.PersonIdentifierEntity", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("Created");

                    b.Property<DateTime?>("Deleted");

                    b.Property<long?>("PersonId")
                        .IsRequired();

                    b.Property<string>("TypeName")
                        .IsRequired();

                    b.Property<string>("Value")
                        .HasMaxLength(2147483647);

                    b.Property<string>("ValueType")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("PersonId");

                    b.ToTable("PersonIdentifier");
                });

            modelBuilder.Entity("CHC.Consent.EFCore.Entities.PersonIdentifierEntity", b =>
                {
                    b.HasOne("CHC.Consent.EFCore.Entities.PersonEntity", "Person")
                        .WithMany()
                        .HasForeignKey("PersonId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
