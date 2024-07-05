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

            modelBuilder.Entity("HNGBackendTwo.Models.OrganisationModel", b =>
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

            modelBuilder.Entity("HNGBackendTwo.Models.UserModel", b =>
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

            modelBuilder.Entity("OrganisationModelUserModel", b =>
                {
                    b.Property<string>("OrganisationsOrgId")
                        .HasColumnType("text");

                    b.Property<string>("UsersUserId")
                        .HasColumnType("text");

                    b.HasKey("OrganisationsOrgId", "UsersUserId");

                    b.HasIndex("UsersUserId");

                    b.ToTable("OrganisationModelUserModel");
                });

            modelBuilder.Entity("OrganisationModelUserModel", b =>
                {
                    b.HasOne("HNGBackendTwo.Models.OrganisationModel", null)
                        .WithMany()
                        .HasForeignKey("OrganisationsOrgId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("HNGBackendTwo.Models.UserModel", null)
                        .WithMany()
                        .HasForeignKey("UsersUserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
