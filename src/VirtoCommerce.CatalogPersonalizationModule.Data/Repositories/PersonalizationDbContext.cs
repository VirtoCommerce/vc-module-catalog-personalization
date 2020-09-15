using EntityFrameworkCore.Triggers;
using Microsoft.EntityFrameworkCore;
using VirtoCommerce.CatalogPersonalizationModule.Data.Model;

namespace VirtoCommerce.CatalogPersonalizationModule.Data.Repositories
{
    public class PersonalizationDbContext : DbContextWithTriggers
    {
        public PersonalizationDbContext(DbContextOptions<PersonalizationDbContext> options) : base(options)
        {
        }

        protected PersonalizationDbContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<TaggedItemEntity>().ToTable("TaggedItem").HasKey(x => x.Id);
            modelBuilder.Entity<TaggedItemEntity>().Property(x => x.Id).HasMaxLength(128).ValueGeneratedOnAdd();
            modelBuilder.Entity<TaggedItemEntity>().HasIndex(x => new { x.ObjectId, x.ObjectType })
                .IsUnique(true)
                .HasName("IX_ObjectId_ObjectType");

            modelBuilder.Entity<TagEntity>().ToTable("Tag").HasKey(x => x.Id);
            modelBuilder.Entity<TagEntity>().Property(x => x.Id).HasMaxLength(128).ValueGeneratedOnAdd();
            modelBuilder.Entity<TagEntity>().HasOne(m => m.TaggedItem).WithMany(x => x.Tags).HasForeignKey(x => x.TaggedItemId)
                .IsRequired().OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TaggedItemOutlineEntity>().ToTable("TaggedItemOutline").HasKey(x => x.Id);
            modelBuilder.Entity<TaggedItemOutlineEntity>().Property(x => x.Id).HasMaxLength(128).ValueGeneratedOnAdd();
            modelBuilder.Entity<TaggedItemOutlineEntity>().HasOne(m => m.TaggedItem).WithMany(x => x.Outlines).HasForeignKey(x => x.TaggedItemId)
                .OnDelete(DeleteBehavior.Cascade);

            // Adding index to Outline column for faster search
            modelBuilder.Entity<TaggedItemOutlineEntity>().HasIndex(x => x.Outline).IsUnique(false).HasName("IX_Outline");

            base.OnModelCreating(modelBuilder);
        }
    }
}
