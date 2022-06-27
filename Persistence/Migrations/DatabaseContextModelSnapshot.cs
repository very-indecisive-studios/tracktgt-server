﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Persistence;

#nullable disable

namespace Persistence.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    partial class DatabaseContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.4")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("Domain.Activity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)");

                    b.Property<DateTime>("CreatedOn")
                        .HasColumnType("datetime(6)");

                    b.Property<DateTime?>("LastModifiedOn")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("MediaRemoteId")
                        .HasColumnType("longtext");

                    b.Property<string>("MediaType")
                        .HasColumnType("longtext");

                    b.Property<bool>("NewlyAdded")
                        .HasColumnType("tinyint(1)");

                    b.Property<int>("NoOf")
                        .HasColumnType("int");

                    b.Property<string>("Status")
                        .HasColumnType("longtext");

                    b.Property<string>("UserRemoteId")
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.ToTable("Activities");
                });

            modelBuilder.Entity("Domain.Book", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)");

                    b.Property<string>("AuthorsString")
                        .HasColumnType("longtext");

                    b.Property<string>("CoverImageURL")
                        .HasColumnType("longtext");

                    b.Property<DateTime>("CreatedOn")
                        .HasColumnType("datetime(6)");

                    b.Property<DateTime?>("LastModifiedOn")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("RemoteId")
                        .HasColumnType("longtext");

                    b.Property<string>("Summary")
                        .HasColumnType("longtext");

                    b.Property<string>("Title")
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.ToTable("Books");
                });

            modelBuilder.Entity("Domain.BookTracking", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)");

                    b.Property<string>("BookRemoteId")
                        .HasColumnType("longtext");

                    b.Property<int>("ChaptersRead")
                        .HasColumnType("int");

                    b.Property<DateTime>("CreatedOn")
                        .HasColumnType("datetime(6)");

                    b.Property<int>("Format")
                        .HasColumnType("int");

                    b.Property<DateTime?>("LastModifiedOn")
                        .HasColumnType("datetime(6)");

                    b.Property<int>("Ownership")
                        .HasColumnType("int");

                    b.Property<int>("Status")
                        .HasColumnType("int");

                    b.Property<string>("UserRemoteId")
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.ToTable("BookTrackings");
                });

            modelBuilder.Entity("Domain.BookWishlist", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)");

                    b.Property<string>("BookRemoteId")
                        .HasColumnType("longtext");

                    b.Property<DateTime>("CreatedOn")
                        .HasColumnType("datetime(6)");

                    b.Property<DateTime?>("LastModifiedOn")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("UserRemoteId")
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.ToTable("BookWishlists");
                });

            modelBuilder.Entity("Domain.Follow", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)");

                    b.Property<DateTime>("CreatedOn")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("FollowerUserId")
                        .HasColumnType("longtext");

                    b.Property<string>("FollowingUserId")
                        .HasColumnType("longtext");

                    b.Property<DateTime?>("LastModifiedOn")
                        .HasColumnType("datetime(6)");

                    b.HasKey("Id");

                    b.ToTable("Follows");
                });

            modelBuilder.Entity("Domain.Game", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)");

                    b.Property<string>("CompaniesString")
                        .HasColumnType("longtext");

                    b.Property<string>("CoverImageURL")
                        .HasColumnType("longtext");

                    b.Property<DateTime>("CreatedOn")
                        .HasColumnType("datetime(6)");

                    b.Property<DateTime?>("LastModifiedOn")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("PlatformsString")
                        .HasColumnType("longtext");

                    b.Property<double>("Rating")
                        .HasColumnType("double");

                    b.Property<long>("RemoteId")
                        .HasColumnType("bigint");

                    b.Property<string>("Summary")
                        .HasColumnType("longtext");

                    b.Property<string>("Title")
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.ToTable("Games");
                });

            modelBuilder.Entity("Domain.GameTracking", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)");

                    b.Property<DateTime>("CreatedOn")
                        .HasColumnType("datetime(6)");

                    b.Property<int>("Format")
                        .HasColumnType("int");

                    b.Property<long>("GameRemoteId")
                        .HasColumnType("bigint");

                    b.Property<float>("HoursPlayed")
                        .HasColumnType("float");

                    b.Property<DateTime?>("LastModifiedOn")
                        .HasColumnType("datetime(6)");

                    b.Property<int>("Ownership")
                        .HasColumnType("int");

                    b.Property<string>("Platform")
                        .HasColumnType("longtext");

                    b.Property<int>("Status")
                        .HasColumnType("int");

                    b.Property<string>("UserRemoteId")
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.ToTable("GameTrackings");
                });

            modelBuilder.Entity("Domain.GameWishlist", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)");

                    b.Property<DateTime>("CreatedOn")
                        .HasColumnType("datetime(6)");

                    b.Property<long>("GameRemoteId")
                        .HasColumnType("bigint");

                    b.Property<DateTime?>("LastModifiedOn")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Platform")
                        .HasColumnType("longtext");

                    b.Property<string>("UserRemoteId")
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.ToTable("GameWishlists");
                });

            modelBuilder.Entity("Domain.Show", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)");

                    b.Property<string>("CoverImageURL")
                        .HasColumnType("longtext");

                    b.Property<DateTime>("CreatedOn")
                        .HasColumnType("datetime(6)");

                    b.Property<DateTime?>("LastModifiedOn")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("RemoteId")
                        .HasColumnType("longtext");

                    b.Property<int>("ShowType")
                        .HasColumnType("int");

                    b.Property<string>("Summary")
                        .HasColumnType("longtext");

                    b.Property<string>("Title")
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.ToTable("Shows");
                });

            modelBuilder.Entity("Domain.ShowTracking", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)");

                    b.Property<DateTime>("CreatedOn")
                        .HasColumnType("datetime(6)");

                    b.Property<int>("EpisodesWatched")
                        .HasColumnType("int");

                    b.Property<DateTime?>("LastModifiedOn")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("ShowRemoteId")
                        .HasColumnType("longtext");

                    b.Property<int>("ShowType")
                        .HasColumnType("int");

                    b.Property<int>("Status")
                        .HasColumnType("int");

                    b.Property<string>("UserRemoteId")
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.ToTable("ShowTrackings");
                });

            modelBuilder.Entity("Domain.User", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)");

                    b.Property<string>("Bio")
                        .HasColumnType("longtext");

                    b.Property<DateTime>("CreatedOn")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Email")
                        .HasColumnType("longtext");

                    b.Property<DateTime?>("LastModifiedOn")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("ProfilePictureURL")
                        .HasColumnType("longtext");

                    b.Property<string>("RemoteId")
                        .HasColumnType("longtext");

                    b.Property<string>("UserName")
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });
#pragma warning restore 612, 618
        }
    }
}
