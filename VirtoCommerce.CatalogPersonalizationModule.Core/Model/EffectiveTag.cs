namespace VirtoCommerce.CatalogPersonalizationModule.Core.Model
{
    public class EffectiveTag
    {
        public string Tag { get; set; }
        public bool IsInherited { get; set; }

        public static EffectiveTag InheritedTag(string tag)
        {
            return new EffectiveTag()
            {
                IsInherited = true,
                Tag = tag
            };
        }

        public static EffectiveTag NonInheritedTag(string tag)
        {
            return new EffectiveTag()
            {
                IsInherited = false,
                Tag = tag
            };
        }
    }
}
