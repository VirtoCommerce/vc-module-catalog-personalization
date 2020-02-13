// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using VirtoCommerce.CatalogPersonalizationModule.Data.Repositories;

namespace VirtoCommerce.CatalogPersonalizationModule.Data.Migrations
{
    [DbContext(typeof(PersonalizationDbContext))]
    partial class PersonalizationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("VirtoCommerce.CatalogPersonalizationModule.Data.Model.TagEntity", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("nvarchar(128)")
                        .HasMaxLength(128);

                    b.Property<string>("Tag")
                        .IsRequired()
                        .HasColumnType("nvarchar(128)")
                        .HasMaxLength(128);

                    b.Property<string>("TaggedItemId")
                        .IsRequired()
                        .HasColumnType("nvarchar(128)")
                        .HasMaxLength(128);

                    b.HasKey("Id");

                    b.HasIndex("TaggedItemId");

                    b.ToTable("Tag");
                });

            modelBuilder.Entity("VirtoCommerce.CatalogPersonalizationModule.Data.Model.TaggedItemEntity", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("nvarchar(128)")
                        .HasMaxLength(128);

                    b.Property<string>("CreatedBy")
                        .HasColumnType("nvarchar(64)")
                        .HasMaxLength(64);

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("Label")
                        .IsRequired()
                        .HasColumnType("nvarchar(128)")
                        .HasMaxLength(128);

                    b.Property<string>("ModifiedBy")
                        .HasColumnType("nvarchar(64)")
                        .HasMaxLength(64);

                    b.Property<DateTime?>("ModifiedDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("ObjectId")
                        .IsRequired()
                        .HasColumnType("nvarchar(128)")
                        .HasMaxLength(128);

                    b.Property<string>("ObjectType")
                        .IsRequired()
                        .HasColumnType("nvarchar(128)")
                        .HasMaxLength(128);

                    b.HasKey("Id");

                    b.ToTable("TaggedItem");
                });

            modelBuilder.Entity("VirtoCommerce.CatalogPersonalizationModule.Data.Model.TaggedItemOutlineEntity", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("nvarchar(128)")
                        .HasMaxLength(128);

                    b.Property<string>("Outline")
                        .IsRequired()
                        .HasColumnType("nvarchar(2048)")
                        .HasMaxLength(2048);

                    b.Property<string>("TaggedItemId")
                        .IsRequired()
                        .HasColumnType("nvarchar(128)")
                        .HasMaxLength(128);

                    b.HasKey("Id");

                    b.HasIndex("Outline");

                    b.HasIndex("TaggedItemId");

                    b.ToTable("TaggedItemOutline");
                });

            modelBuilder.Entity("VirtoCommerce.CatalogPersonalizationModule.Data.Model.TagEntity", b =>
                {
                    b.HasOne("VirtoCommerce.CatalogPersonalizationModule.Data.Model.TaggedItemEntity", "TaggedItem")
                        .WithMany("Tags")
                        .HasForeignKey("TaggedItemId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("VirtoCommerce.CatalogPersonalizationModule.Data.Model.TaggedItemOutlineEntity", b =>
                {
                    b.HasOne("VirtoCommerce.CatalogPersonalizationModule.Data.Model.TaggedItemEntity", "TaggedItem")
                        .WithMany("Outlines")
                        .HasForeignKey("TaggedItemId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
