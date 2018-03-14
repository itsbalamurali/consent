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
    [Migration("20180314160843_ConsentHeader")]
    partial class ConsentHeader
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.0.1-rtm-125")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("CHC.Consent.EFCore.Consent.ConsentEntity", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("DateProvided");

                    b.Property<DateTime?>("DateWithdrawn");

                    b.Property<long>("GivenByPersonId");

                    b.Property<long>("StudySubjectId");

                    b.HasKey("Id");

                    b.HasIndex("GivenByPersonId");

                    b.HasIndex("StudySubjectId", "DateProvided", "DateWithdrawn")
                        .IsUnique()
                        .HasFilter("[DateWithdrawn] IS NOT NULL");

                    b.ToTable("Consent");
                });

            modelBuilder.Entity("CHC.Consent.EFCore.Consent.StudyEntity", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name")
                        .IsRequired();

                    b.HasKey("Id");

                    b.ToTable("Study");
                });

            modelBuilder.Entity("CHC.Consent.EFCore.Consent.StudySubjectEntity", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<long>("PersonId");

                    b.Property<long>("StudyId");

                    b.Property<string>("SubjectIdentifier")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("PersonId");

                    b.HasIndex("StudyId", "PersonId")
                        .IsUnique();

                    b.HasIndex("StudyId", "SubjectIdentifier")
                        .IsUnique();

                    b.ToTable("StudySubject");
                });

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

            modelBuilder.Entity("CHC.Consent.EFCore.Consent.ConsentEntity", b =>
                {
                    b.HasOne("CHC.Consent.EFCore.Entities.PersonEntity", "GivenBy")
                        .WithMany()
                        .HasForeignKey("GivenByPersonId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("CHC.Consent.EFCore.Consent.StudySubjectEntity", "StudySubject")
                        .WithMany()
                        .HasForeignKey("StudySubjectId")
                        .OnDelete(DeleteBehavior.SetNull);
                });

            modelBuilder.Entity("CHC.Consent.EFCore.Consent.StudySubjectEntity", b =>
                {
                    b.HasOne("CHC.Consent.EFCore.Entities.PersonEntity", "Person")
                        .WithMany()
                        .HasForeignKey("PersonId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("CHC.Consent.EFCore.Consent.StudyEntity", "Study")
                        .WithMany()
                        .HasForeignKey("StudyId")
                        .OnDelete(DeleteBehavior.Cascade);
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
