using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using VirtoCommerce.CatalogPersonalizationModule.Core.Model;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogPersonalizationModule.Data.Model
{
    public class TaggedItemEntity : AuditableEntity
    {
        public TaggedItemEntity()
        {
            Tags = new NullCollection<TagEntity>();
        }

        [Required]
        [StringLength(128)]
        public string Label { get; set; }

        [Required]
        [StringLength(128)]
        public string ObjectType { get; set; }

        [Required]
        [StringLength(128)]
        public string ObjectId { get; set; }

        #region Navigation Properties

        public virtual ObservableCollection<TagEntity> Tags { get; set; }

        #endregion

        public virtual TaggedItem ToModel(TaggedItem taggedItem)
        {
            if (taggedItem == null)
                throw new ArgumentNullException(nameof(taggedItem));

            taggedItem.Id = Id;

            taggedItem.CreatedBy = CreatedBy;
            taggedItem.CreatedDate = CreatedDate;
            taggedItem.ModifiedBy = ModifiedBy;
            taggedItem.ModifiedDate = ModifiedDate;

            taggedItem.Label = Label;
            taggedItem.EntityType = ObjectType;
            taggedItem.EntityId = ObjectId;
            taggedItem.Tags = Tags.Select(x => x.Tag).ToList();

            return taggedItem;
        }

        public virtual TaggedItemEntity FromModel(TaggedItem taggedItem, PrimaryKeyResolvingMap pkMap)
        {
            if (taggedItem == null)
                throw new ArgumentNullException(nameof(taggedItem));
            if (pkMap == null)
                throw new ArgumentNullException(nameof(pkMap));

            pkMap.AddPair(taggedItem, this);

            Id = taggedItem.Id;

            CreatedBy = taggedItem.CreatedBy;
            CreatedDate = taggedItem.CreatedDate;
            ModifiedBy = taggedItem.ModifiedBy;
            ModifiedDate = taggedItem.ModifiedDate;

            Label = taggedItem.Label;
            ObjectType = taggedItem.EntityType;
            ObjectId = taggedItem.EntityId;

            if (taggedItem.Tags != null)
            {
                Tags = new ObservableCollection<TagEntity>(taggedItem.Tags.Select(x => new TagEntity
                {
                    Tag = x
                }));
            }

            return this;
        }

        public virtual void Patch(TaggedItemEntity taggedItemEntity)
        {
            if (taggedItemEntity == null)
                throw new ArgumentNullException(nameof(taggedItemEntity));

            taggedItemEntity.Label = Label;
            taggedItemEntity.ObjectType = ObjectType;
            taggedItemEntity.ObjectId = ObjectId;

            if (!Tags.IsNullCollection())
            {
                var tagComparer = AnonymousComparer.Create((TagEntity x) => x.Tag);
                Tags.Patch(taggedItemEntity.Tags, tagComparer, (sourceTag, targetTag) => targetTag.Tag = sourceTag.Tag);
            }
        }
    }
}
