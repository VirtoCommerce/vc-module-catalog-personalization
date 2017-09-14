using System.ComponentModel.DataAnnotations;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogPersonalizationModule.Data.Model
{
    public class TagEntity : Entity
    {
        [Required]
        public string Tag { get; set; }

        #region Navigation Properties

        public string TaggedItemId { get; set; }

        public TaggedItemEntity TaggedItem { get; set; }

        #endregion
    }
}
