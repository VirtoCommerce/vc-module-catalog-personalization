using System.Linq;
using VirtoCommerce.CatalogPersonalizationModule.Data.Model;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogPersonalizationModule.Data.Repositories
{
	public interface IPersonalizationRepository : IRepository
	{
		IQueryable<TaggedItemEntity> TaggedItems { get; }
		IQueryable<TagEntity> Tags { get; }
		IQueryable<TaggedItemOutlineEntity> TaggedItemOutlines { get; }

		TaggedItemEntity[] GetTaggedItemsByIds(string[] ids, string responseGroup);
        void DeleteTaggedItems(string[] ids);

	}
}
