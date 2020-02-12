using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.CatalogPersonalizationModule.Core.Model;
using VirtoCommerce.CatalogPersonalizationModule.Core.Services;
using VirtoCommerce.CatalogPersonalizationModule.Data.Common;
using VirtoCommerce.CatalogPersonalizationModule.Data.Repositories;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Data.Infrastructure;
using VirtoCommerce.SearchModule.Core.Model;

namespace VirtoCommerce.CatalogPersonalizationModule.Data.Services
{
    public class UpTreeTagPropagationPolicy : TreeTagPropagationPolicy, ITagPropagationPolicy
    {
        private readonly Func<IPersonalizationRepository> _repositoryFactory;
        private readonly IListEntrySearchService _listEntrySearchService;
        
        public UpTreeTagPropagationPolicy(
            Func<IPersonalizationRepository> repositoryFactory,
            IListEntrySearchService listEntrySearchService) : base(repositoryFactory)
        {
            _repositoryFactory = repositoryFactory;
            _listEntrySearchService = listEntrySearchService;
            
        }

        public async Task<Dictionary<string, List<EffectiveTag>>> GetResultingTagsAsync(IEntity[] entities)
        {
            if (entities == null)
            {
                throw new ArgumentNullException(nameof(entities));
            }

            var result = entities.ToDictionary(x => x.Id, x => new List<EffectiveTag>());

            var entitiesIds = entities.Select(x => x.Id).ToArray();


            using (var repository = _repositoryFactory())
            {
                repository.DisableChangesTracking();

                //Loading own tags for given entities
                var taggedItemIds = repository.TaggedItems.Where(x => entitiesIds.Contains(x.ObjectId))
                                                          .Select(x => x.Id)
                                                          .ToArray();

                var taggedItems = await GetTaggedItemsByIdsAsync(taggedItemIds);
                foreach (var taggedItem in taggedItems)
                {
                    result[taggedItem.EntityId].AddRange(taggedItem.Tags.Select(x => EffectiveTag.NonInheritedTag(x)));
                }
                //Trying to propagate  tags from children objects, use for this the saved outlines of tagged items 
                foreach (var category in entities.OfType<Category>())
                {
                    if (!category.Outlines.IsNullOrEmpty())
                    {
                        var outlines = category.Outlines.Select(x => x.ToString())
                                                   .Distinct(StringComparer.OrdinalIgnoreCase)
                                                   .ToArray();
                        //load all descendants tag items for this category. Use for this the stored outlines
                        taggedItemIds = repository.TaggedItemOutlines.Where(i => outlines.Any(o => i.Outline.StartsWith(o)))
                                                                       .Select(i => i.TaggedItem.Id)
                                                                       .Distinct()
                                                                       .ToArray();

                        taggedItems = await GetTaggedItemsByIdsAsync(taggedItemIds);
                        result[category.Id].AddRange(taggedItems.SelectMany(x => x.Tags.Select(y => EffectiveTag.InheritedTag(y))));

                        //Also need to propagate __any tag up the hierarchy if category contains  products that aren't tagged
                        //Using for this a comparison  of the count of tagged products in the module storage and the real products count from original store of catalog subsystem 
                        //TODO: Find a better solution hot to known what category contains not tagged products
                        var criteria = new CatalogListEntrySearchCriteria
                        {
                            ObjectType = nameof(CatalogProduct),
                            ResponseGroup = ItemResponseGroup.ItemInfo.ToString(),
                            CategoryId = category.Id,
                            SearchInChildren = true,
                            SearchInVariations = true,
                            WithHidden = true,
                            Take = 0,
                        };
                        var allCategoryProductsCount = (await _listEntrySearchService.SearchAsync(criteria)).ListEntries.Count;
                        var allCategoryTaggedProductsCount = taggedItems.Count(x => x.EntityType.EqualsInvariant(KnownDocumentTypes.Product) && x.Tags.Any());
                        if (allCategoryProductsCount > allCategoryTaggedProductsCount)
                        {
                            result[category.Id].Add(EffectiveTag.InheritedTag(Constants.UserGroupsAnyValue));
                        }

                        // Need to remove duplicates
                        result[category.Id] = result[category.Id].Distinct(AnonymousComparer.Create<EffectiveTag, string>(x => $"{x.Tag}:{x.IsInherited}")).ToList();
                    }
                }
            }
            return result;
        }
    }
}
