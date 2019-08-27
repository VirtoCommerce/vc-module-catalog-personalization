using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.SqlClient;
using System.Linq;
using VirtoCommerce.CatalogPersonalizationModule.Core.Model.Search;
using VirtoCommerce.CatalogPersonalizationModule.Data.Model;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Data.Infrastructure;
using VirtoCommerce.Platform.Data.Infrastructure.Interceptors;

namespace VirtoCommerce.CatalogPersonalizationModule.Data.Repositories
{
    public class PersonalizationRepositoryImpl : EFRepositoryBase, IPersonalizationRepository
    {
        public PersonalizationRepositoryImpl()
        {
        }

        public PersonalizationRepositoryImpl(string nameOrConnectionString, params IInterceptor[] interceptors)
            : base(nameOrConnectionString, null, interceptors)
        {
            Configuration.LazyLoadingEnabled = false;
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TagEntity>().HasKey(x => x.Id).Property(x => x.Id);
            modelBuilder.Entity<TagEntity>().HasRequired(x => x.TaggedItem).WithMany(x => x.Tags).HasForeignKey(x => x.TaggedItemId).WillCascadeOnDelete(true);
            modelBuilder.Entity<TagEntity>().ToTable("Tag");

            modelBuilder.Entity<TaggedItemOutlineEntity>().HasKey(x => x.Id).Property(x => x.Id);
            modelBuilder.Entity<TaggedItemOutlineEntity>().HasRequired(x => x.TaggedItem).WithMany(x => x.Outlines).HasForeignKey(x => x.TaggedItemId).WillCascadeOnDelete(true);
            // Adding index to Outline column for faster search
            modelBuilder.Entity<TaggedItemOutlineEntity>().Property(x => x.Outline)
                .HasColumnAnnotation(IndexAnnotation.AnnotationName, new IndexAnnotation(new IndexAttribute() { IsUnique = false }));
            modelBuilder.Entity<TaggedItemOutlineEntity>().ToTable("TaggedItemOutline");

            modelBuilder.Entity<TaggedItemEntity>().HasKey(x => x.Id).Property(x => x.Id);
            modelBuilder.Entity<TaggedItemEntity>().ToTable("TaggedItem");

            base.OnModelCreating(modelBuilder);
        }

        public IQueryable<TaggedItemEntity> TaggedItems => GetAsQueryable<TaggedItemEntity>().Include(x => x.Tags);

        public IQueryable<TagEntity> Tags => GetAsQueryable<TagEntity>();

        public IQueryable<TaggedItemOutlineEntity> TaggedItemOutlines => GetAsQueryable<TaggedItemOutlineEntity>();

        public TaggedItemEntity[] GetTaggedItemsByIds(string[] ids, string responseGroup)
        {
            var result = Array.Empty<TaggedItemEntity>();
            if (!ids.IsNullOrEmpty())
            {
                var taggedItemrespGroup = EnumUtility.SafeParse(responseGroup, TaggedItemResponseGroup.Full);

                result = TaggedItems.Where(x => ids.Contains(x.Id)).ToArray();

                if (taggedItemrespGroup.HasFlag(TaggedItemResponseGroup.WithOutlines))
                {
                    var outlines = TaggedItemOutlines.Where(x => ids.Contains(x.TaggedItemId)).ToArray();
                }
            }
            return result;
        }

        public void DeleteTaggedItems(string[] ids)
        {
            ExecuteStoreCommand("DELETE FROM TaggedItem WHERE Id IN ({0})", ids);
        }

        protected virtual void ExecuteStoreCommand(string commandTemplate, IEnumerable<string> parameterValues)
        {
            var command = CreateCommand(commandTemplate, parameterValues);
            ObjectContext.ExecuteStoreCommand(command.Text, command.Parameters);
        }

        protected virtual Command CreateCommand(string commandTemplate, IEnumerable<string> parameterValues)
        {
            var parameters = parameterValues.Select((v, i) => new SqlParameter($"@p{i}", v)).ToArray();
            var parameterNames = string.Join(",", parameters.Select(p => p.ParameterName));

            return new Command
            {
                Text = string.Format(commandTemplate, parameterNames),
                Parameters = parameters.OfType<object>().ToArray(),
            };
        }


        public void DeleteTaggedItemOutlines(string[] ids)
        {
            ExecuteStoreCommand("DELETE FROM [TaggedItemOutline] WHERE Id IN ({0})", ids);
        }

        protected class Command
        {
            public string Text { get; set; }
            public object[] Parameters { get; set; }
        }
    }
}
