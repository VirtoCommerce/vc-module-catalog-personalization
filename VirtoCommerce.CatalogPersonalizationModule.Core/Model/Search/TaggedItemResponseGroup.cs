namespace VirtoCommerce.CatalogPersonalizationModule.Core.Model.Search
{
    public enum TaggedItemResponseGroup
    {
        None = 0,
        Info = 1,
        WithOutlines = 1 << 1,
        Full = Info | WithOutlines
	}
}
