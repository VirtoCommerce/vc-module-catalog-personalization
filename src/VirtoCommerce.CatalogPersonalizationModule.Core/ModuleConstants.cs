using System.Collections.Generic;
using VirtoCommerce.Platform.Core.Settings;

namespace VirtoCommerce.CatalogPersonalizationModule.Core
{
    public static class ModuleConstants
    {
        public static class Security
        {
            public static class Permissions
            {
                public const string Update = "personalization:update";
                public static readonly string[] AllPermissions = { Update };
            }
        }

        public static class Settings
        {
            public static class General
            {
                public readonly static SettingDescriptor TagsInheritancePolicy = new SettingDescriptor
                {
                    Name = "CatalogPersonalization.TagsInheritancePolicy",
                    GroupName = "Personalization|General",
                    ValueType = SettingValueType.ShortText,
                    DefaultValue = "DownTree",
                    AllowedValues = new string[] { "DownTree", "UpTree" }
                };

                public readonly static SettingDescriptor CronExpression = new SettingDescriptor
                {
                    Name = "CatalogPersonalization.CronExpression",
                    GroupName = "Personalization|General",
                    ValueType = SettingValueType.ShortText,
                    DefaultValue = "0/15 * * * *"
                };

                public static SettingDescriptor LogTaggedItemsChanges { get; } = new SettingDescriptor
                {
                    Name = "CatalogPersonalization.LogTaggedItemsChanges",
                    GroupName = "Personalization|General",
                    ValueType = SettingValueType.Boolean,
                    DefaultValue = false,
                };

                public static SettingDescriptor EventBasedIndexation { get; } = new SettingDescriptor
                {
                    Name = "CatalogPersonalization.EventBasedIndexation",
                    GroupName = "Personalization|General",
                    ValueType = SettingValueType.Boolean,
                    DefaultValue = false,
                };

                public static IEnumerable<SettingDescriptor> AllSettings
                {
                    get
                    {
                        return new List<SettingDescriptor>
                        {
                            TagsInheritancePolicy,
                            CronExpression,
                            EventBasedIndexation,
                            LogTaggedItemsChanges
                        };
                    }
                }
            }
        }
    }
}
