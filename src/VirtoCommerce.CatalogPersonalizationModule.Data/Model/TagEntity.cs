using System.ComponentModel.DataAnnotations;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogPersonalizationModule.Data.Model
{
    public class TagEntity : Entity
    {
        [Required]
        [StringLength(128)]
        public string Tag { get; set; }

        #region Navigation Properties
        [Required]
        [StringLength(128)]
        public string TaggedItemId { get; set; }

        public virtual TaggedItemEntity TaggedItem { get; set; }

        #endregion
    }
}
