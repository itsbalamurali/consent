﻿// <auto-generated />
using CHC.Consent.Common;
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
    [Migration("20180219145838_BradfordHospitalNumbers")]
    partial class BradfordHospitalNumbers
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.0.1-rtm-125")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("CHC.Consent.EFCore.BradfordHospitalNumberEntity", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("HospitalNumber");

                    b.Property<long?>("PersonEntityId");

                    b.HasKey("Id");

                    b.HasIndex("PersonEntityId");

                    b.ToTable("BradfordHospitalNumberEntity");
                });

            modelBuilder.Entity("CHC.Consent.EFCore.PersonEntity", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int?>("BirthOrderValue")
                        .HasColumnName("BirthOrder");

                    b.Property<DateTime?>("DateOfBirth");

                    b.Property<string>("NhsNumber");

                    b.Property<int?>("Sex");

                    b.HasKey("Id");

                    b.ToTable("People");
                });

            modelBuilder.Entity("CHC.Consent.EFCore.BradfordHospitalNumberEntity", b =>
                {
                    b.HasOne("CHC.Consent.EFCore.PersonEntity")
                        .WithMany("BradfordHospitalNumberEntities")
                        .HasForeignKey("PersonEntityId");
                });
#pragma warning restore 612, 618
        }
    }
}
