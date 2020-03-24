using System.Collections.Generic;
using VirtoCommerce.Platform.Core.Settings;

namespace VirtoCommerce.CatalogPersonalizationModule.Core
{
    public class ModuleConstants
    {
        public static class Security
        {
            public static class Permissions
            {
                public const string Update = "personalization:update";
                public static string[] AllPermissions = { Update };
            }
        }

        public static class Settings
        {
            public static class General
            {
                public static SettingDescriptor TagsInheritancePolicy = new SettingDescriptor
                {
                    Name = "CatalogPersonalization.TagsInheritancePolicy",
                    GroupName = "Personalization|General",
                    ValueType = SettingValueType.ShortText,
                    DefaultValue = "DownTree",
                    AllowedValues = new string[] { "DownTree", "UpTree" },
                    RestartRequired = true
                };

                public static SettingDescriptor CronExpression = new SettingDescriptor
                {
                    Name = "CatalogPersonalization.CronExpression",
                    GroupName = "Personalization|General",
                    ValueType = SettingValueType.ShortText,
                    DefaultValue = "0/15 * * * *",
                    RestartRequired = true
                };

                public static IEnumerable<SettingDescriptor> AllSettings
                {
                    get
                    {
                        return new List<SettingDescriptor>
                        {
                            TagsInheritancePolicy,
                            CronExpression
                        };
                    }
                }
            }
        }
    }
}
