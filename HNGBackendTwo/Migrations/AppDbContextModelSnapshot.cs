﻿// <auto-generated />
using HNGBackendTwo.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace HNGBackendTwo.Migrations
{
    [DbContext(typeof(AppDbContext))]
    partial class AppDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.6")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("HNGBackendTwo.Models.Organisation", b =>
                {
                    b.Property<string>("OrgId")
                        .HasColumnType("text");

                    b.Property<string>("Description")
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("OrgId");

                    b.ToTable("Organisations");
                });

            modelBuilder.Entity("HNGBackendTwo.Models.OrganisationUser", b =>
                {
                    b.Property<string>("OrganisationId")
                        .HasColumnType("text");

                    b.Property<string>("UserId")
                        .HasColumnType("text");

                    b.HasKey("OrganisationId", "UserId");

                    b.HasIndex("UserId");

                    b.ToTable("OrganisationUsers");
                });

            modelBuilder.Entity("HNGBackendTwo.Models.User", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("text");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Phone")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("UserId");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("HNGBackendTwo.Models.OrganisationUser", b =>
                {
                    b.HasOne("HNGBackendTwo.Models.Organisation", "Organisation")
                        .WithMany("OrganisationUsers")
                        .HasForeignKey("OrganisationId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("HNGBackendTwo.Models.User", "User")
                        .WithMany("OrganisationUsers")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Organisation");

                    b.Navigation("User");
                });

            modelBuilder.Entity("HNGBackendTwo.Models.Organisation", b =>
                {
                    b.Navigation("OrganisationUsers");
                });

            modelBuilder.Entity("HNGBackendTwo.Models.User", b =>
                {
                    b.Navigation("OrganisationUsers");
                });
#pragma warning restore 612, 618
        }
    }
}
