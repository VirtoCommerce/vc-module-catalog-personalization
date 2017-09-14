using System.Collections.Generic;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogPersonalizationModule.Core.Model
{
    public class TaggedItem : AuditableEntity
    {
        public string Label { get; set; }
        public string EntityType { get; set; }
        public string EntityId { get; set; }

        public ICollection<string> Tags { get; set; }
    }
}
