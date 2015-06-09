using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Microsoft.SharePoint.Publishing;

namespace GSoft.Dynamite.Globalization.Variations
{
    /// <summary>
    /// A simple POCO that represent a variation label's definition
    /// </summary>
    public class VariationLabelInfo
    {
        /// <summary>
        /// Empty constructor for serialization purposes
        /// </summary>
        public VariationLabelInfo()
        {
            // Default values
            this.HierarchyCreationMode = HierarchyCreationMode.PublishingSitesAndAllPages;
            this.IsAutomaticUpdate = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CacheVariationLabel"/> class.
        /// </summary>
        /// <param name="variationLabel">The variation label.</param>
        public VariationLabelInfo(VariationLabel variationLabel) : this()
        {
            this.Title = variationLabel.Title;
            this.DisplayName = variationLabel.DisplayName;
            this.IsSource = variationLabel.IsSource;
            this.Language = TryParseCulture(variationLabel.Language);
            this.Locale = TryParseCulture(variationLabel.Locale);
            this.TopWebUrl = new Uri(variationLabel.TopWebUrl);
        }

        /// <summary>
        /// Gets or sets the title of the label. This determines the root-web-relative
        /// URL of the top web in that label's site hierarchy.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the "flag control display name" of this variation label.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the language of the label's web (its CurrentUICulture, i.e. the MUI/Language pack culture).
        /// This setting determines the language of the SharePoint UI.
        /// </summary>
        public CultureInfo Language { get; set; }

        /// <summary>
        /// Gets or sets the locale of the label's web (its CurrentCulture, i.e. the regional settings).
        /// This setting determines, among other things, the date and money formats that will be displayed
        /// in the site.
        /// </summary>
        public CultureInfo Locale { get; set; }

        /// <summary>
        /// Gets or sets the hierarchy creation mode.
        /// </summary>
        public HierarchyCreationMode HierarchyCreationMode { get; set; }

        /// <summary>
        /// If true, this variation label will be the source for all other labels.
        /// Make sure you define only one source label. All other variations should be
        /// target labels.
        /// </summary>
        public bool IsSource { get; set; }

        /// <summary>
        /// Gets or sets the description of the variation label.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// URL of the top PublishingWeb in the label's variations hierarchy.
        /// Unless your site collection's root web is acting as the source label,
        /// this will typically be the URL of a first-level sub-web.
        /// </summary>
        public Uri TopWebUrl { get; set; }

        /// <summary>
        /// This is the "Page Update Behavior" option. In the variation wizard (at /_layouts/15/VariationLabelWizard.aspx)
        /// there is an option for automatic update. Sadly, the Field in the list item is "NotificationMode". 
        /// We decided to keep the UI name here because it is more meaningful.
        /// When IsAutomaticUpdate == true, then each new major version publishing in the source label will lead to
        /// a new draft (minor version) being added automatically to its associated variation target.
        /// When IsAutomaticUpdate == true, then the contributors on the targets labels will only see a notification
        /// that the source variation has been modified.
        /// This setting should be ignored on source labels: it only makes sense to define it for target labels.
        /// </summary>
        public bool IsAutomaticUpdate { get; set; }

        /// <summary>
        /// Gets or Sets the LanguageSwitchCustomTitle property.
        /// Defines the display name of the label when rendered in the language switcher control.
        /// </summary>
        public string LanguageSwitchCustomTitle { get; set; }

        /// <summary>
        /// Gets or Sets the LanguageSwitchCustomCssClass property. Add a css class to the label
        /// when rendered in the language switcher user control.
        /// </summary>
        public string LanguageSwitchCustomCssClass { get; set; }

        private static CultureInfo TryParseCulture(string cultureAsString)
        {
            CultureInfo cultureInfo = null;
            int cultureLCID = 0;
            if (int.TryParse(cultureAsString, out cultureLCID))
            {
                // assume we're dealing with an LCID
                cultureInfo = new CultureInfo(cultureLCID);
            }
            else
            {
                // not an LCID, so assume we're dealing with "fr-FR" culture string format
                cultureInfo = new CultureInfo(cultureAsString);
            }

            return cultureInfo;
        }
    }
}
