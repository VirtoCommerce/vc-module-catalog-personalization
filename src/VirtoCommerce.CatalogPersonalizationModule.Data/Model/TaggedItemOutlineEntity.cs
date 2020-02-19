using System;
using System.ComponentModel.DataAnnotations;
using VirtoCommerce.CatalogPersonalizationModule.Core.Model;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogPersonalizationModule.Data.Model
{
    public class TaggedItemOutlineEntity : Entity
    {
        [Required]
        [StringLength(2048)]
        public string Outline { get; set; }

        #region Navigation Properties
        [Required]
        [StringLength(128)]
        public string TaggedItemId { get; set; }

        public virtual TaggedItemEntity TaggedItem { get; set; }

        #endregion

        public virtual TaggedItemOutline ToModel(TaggedItemOutline taggedItemOutline)
        {
            if (taggedItemOutline == null)
                throw new ArgumentNullException(nameof(taggedItemOutline));

            taggedItemOutline.Id = Id;

            taggedItemOutline.Outline = Outline;
            taggedItemOutline.TaggedItemId = TaggedItemId;

            return taggedItemOutline;
        }

        public virtual TaggedItemOutlineEntity FromModel(TaggedItemOutline taggedItemOutline, PrimaryKeyResolvingMap pkMap)
        {
            if (taggedItemOutline == null)
                throw new ArgumentNullException(nameof(taggedItemOutline));

            if (pkMap == null)
                throw new ArgumentNullException(nameof(pkMap));

            pkMap.AddPair(taggedItemOutline, this);

            Id = taggedItemOutline.Id;

            Outline = taggedItemOutline.Outline;
            TaggedItemId = taggedItemOutline.TaggedItemId;

            return this;
        }

        public virtual void Patch(TaggedItemOutlineEntity taggedItemOutlineEntity)
        {
            if (taggedItemOutlineEntity == null)
                throw new ArgumentNullException(nameof(taggedItemOutlineEntity));

            taggedItemOutlineEntity.Outline = Outline;
            taggedItemOutlineEntity.TaggedItemId = TaggedItemId;
        }
    }
}
