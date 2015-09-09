using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSoft.Dynamite.Globalization.Variations
{
    /// <summary>
    /// The various variation hierarchy creation modes (the initialization behavior 
    /// when you create new target labels)
    /// </summary>
    public enum HierarchyCreationMode
    {
        /// <summary>
        /// When the target variation label site hierarchy will be created,
        /// all of the source label's publishing sites, variated lists and
        /// all pages will be copied to the target.
        /// </summary>
        PublishingSitesAndAllPages,

        /// <summary>
        /// When the target variation label site hierarchy will be created,
        /// only the source label's publishing sites will be copied to the target.
        /// </summary>
        PublishingSitesOnly,

        /// <summary>
        /// When the target variation label site hierarchy will be created,
        /// only the source label's root site will be copied.
        /// </summary>
        RootSitesOnly
    }

    /// <summary>
    /// Extensions for <see cref="HierarchyCreationMode"/>
    /// </summary>
    public static class HierarchyCreationModeExtensions
    {
        /// <summary>
        /// Extension method to easily extract the correct string value
        /// from the various creation modes (as it should be set in the
        /// label's list item in the variation labels list).
        /// </summary>
        /// <param name="self">The enum value we want to convert as string</param>
        /// <returns>The string value that represents the hierarchy creation mode</returns>
        public static string ToListItemValueString(this HierarchyCreationMode self)
        {
            switch (self)
            {
                case HierarchyCreationMode.PublishingSitesOnly:
                    return "Publishing Sites Only";
                case HierarchyCreationMode.RootSitesOnly:
                    return "Root Sites Only";
                default:
                    return "Publishing Sites and All Pages";
            }
        }
    }
}
