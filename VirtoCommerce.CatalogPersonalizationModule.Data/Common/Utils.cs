using System;
using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.CatalogPersonalizationModule.Core.Model;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogPersonalizationModule.Data.Common
{
    public static class Utils
    {
        public static ICollection<T[]> SplitList<T>(this ICollection<T> collection, int size = 50)
        {
            var list = new List<T[]>();
            for (int i = 0; i < collection.Count; i += size)
                list.Add(collection.Skip(i).Take(Math.Min(size, collection.Count - i)).ToArray());
            return list;
        }

        public static string[] GetObjectsOutlineIds<T>(this IList<T> objects) where T : IEntity, IHasOutlines
        {
            var outlineIds = objects.SelectMany(c => GetOutlineIds(c))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
            return outlineIds;
        }

        public static string[] GetOutlineIds(this IHasOutlines outlineObject)
        {
            var outlineIds = outlineObject.Outlines.SelectMany(o => o.Items)
                .Select(o => o.Id)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            return outlineIds;
        }

        public static IDictionary<string, string[]> CombineObjectsWithTags<T>(this IList<T> objects, TaggedItem[] taggedItems) where T : IEntity, IHasOutlines
        {
            IDictionary<string, string[]> combinedData = objects.Select(o =>
            {
                var ids = o.GetOutlineIds();
                var tags = taggedItems.Where(t => ids.Contains(t.EntityId)).SelectMany(t => t.Tags).Distinct(StringComparer.OrdinalIgnoreCase);
                return new { o.Id, Tags = tags.ToArray() };
            }).ToDictionary(d => d.Id, d => d.Tags);

            return combinedData;
        }
    }
}
