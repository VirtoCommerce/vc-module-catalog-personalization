using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VirtoCommerce.CatalogPersonalizationModule.Data.Model;

namespace VirtoCommerce.CatalogPersonalizationModule.Data.MySql
{
    internal class TaggedItemOutlineEntityConfiguration : IEntityTypeConfiguration<TaggedItemOutlineEntity>
    {
        public void Configure(EntityTypeBuilder<TaggedItemOutlineEntity> builder)
        {
            builder.Property(x => x.Outline).HasMaxLength(700);
        }
    }

}
