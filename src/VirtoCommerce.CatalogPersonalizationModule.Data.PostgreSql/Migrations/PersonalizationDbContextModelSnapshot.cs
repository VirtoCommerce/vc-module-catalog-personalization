﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using VirtoCommerce.CatalogPersonalizationModule.Data.Repositories;

#nullable disable

namespace VirtoCommerce.CatalogPersonalizationModule.Data.PostgreSql.Migrations
{
    [DbContext(typeof(PersonalizationDbContext))]
    partial class PersonalizationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("VirtoCommerce.CatalogPersonalizationModule.Data.Model.TagEntity", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)");

                    b.Property<string>("Tag")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)");

                    b.Property<string>("TaggedItemId")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)");

                    b.HasKey("Id");

                    b.HasIndex("TaggedItemId");

                    b.ToTable("Tag", (string)null);
                });

            modelBuilder.Entity("VirtoCommerce.CatalogPersonalizationModule.Data.Model.TaggedItemEntity", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)");

                    b.Property<string>("CreatedBy")
                        .HasMaxLength(64)
                        .HasColumnType("character varying(64)");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Label")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)");

                    b.Property<string>("ModifiedBy")
                        .HasMaxLength(64)
                        .HasColumnType("character varying(64)");

                    b.Property<DateTime?>("ModifiedDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("ObjectId")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)");

                    b.Property<string>("ObjectType")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)");

                    b.HasKey("Id");

                    b.HasIndex("ObjectId", "ObjectType")
                        .IsUnique()
                        .HasDatabaseName("IX_TaggedItem_ObjectId_ObjectType");

                    b.ToTable("TaggedItem", (string)null);
                });

            modelBuilder.Entity("VirtoCommerce.CatalogPersonalizationModule.Data.Model.TaggedItemOutlineEntity", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)");

                    b.Property<string>("Outline")
                        .IsRequired()
                        .HasMaxLength(2048)
                        .HasColumnType("character varying(2048)");

                    b.Property<string>("TaggedItemId")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)");

                    b.HasKey("Id");

                    b.HasIndex("Outline")
                        .HasDatabaseName("IX_TaggedItemOutlineEntity_Outline");

                    b.HasIndex("TaggedItemId");

                    b.ToTable("TaggedItemOutline", (string)null);
                });

            modelBuilder.Entity("VirtoCommerce.CatalogPersonalizationModule.Data.Model.TagEntity", b =>
                {
                    b.HasOne("VirtoCommerce.CatalogPersonalizationModule.Data.Model.TaggedItemEntity", "TaggedItem")
                        .WithMany("Tags")
                        .HasForeignKey("TaggedItemId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("TaggedItem");
                });

            modelBuilder.Entity("VirtoCommerce.CatalogPersonalizationModule.Data.Model.TaggedItemOutlineEntity", b =>
                {
                    b.HasOne("VirtoCommerce.CatalogPersonalizationModule.Data.Model.TaggedItemEntity", "TaggedItem")
                        .WithMany("Outlines")
                        .HasForeignKey("TaggedItemId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("TaggedItem");
                });

            modelBuilder.Entity("VirtoCommerce.CatalogPersonalizationModule.Data.Model.TaggedItemEntity", b =>
                {
                    b.Navigation("Outlines");

                    b.Navigation("Tags");
                });
#pragma warning restore 612, 618
        }
    }
}
