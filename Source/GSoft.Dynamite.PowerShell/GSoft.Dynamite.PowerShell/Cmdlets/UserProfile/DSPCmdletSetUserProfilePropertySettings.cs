using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using System.Xml.Linq;
using System.Xml.Serialization;
using GSoft.Dynamite.PowerShell.Cmdlets.UserProfile.Entities;
using GSoft.Dynamite.PowerShell.Extensions;
using GSoft.Dynamite.PowerShell.PipeBindsObjects;
using Microsoft.Office.Server.UserProfiles;
using Microsoft.SharePoint;
using Microsoft.SharePoint.PowerShell;

namespace GSoft.Dynamite.PowerShell.Cmdlets.UserProfile
{
    /// <summary>
    /// Cmdlet for user profile 
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "DSPUserProfilePropertySettings")]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
    public class DSPCmdletSetUserProfilePropertySettings : SPCmdlet
    {
        private XmlSerializer serializer;
        private UserProfileConfigManager profileConfigManager;
        private ProfilePropertyManager profilePropertyManager;
        private CorePropertyManager corePropertyManager;
        private ProfileTypePropertyManager profileTypePropertyManager;
        private ProfileSubtypeManager profileSubTypeManager;
        private ProfileSubtypePropertyManager profileSubTypePropertyManager;
        private bool orderHasChanged;

        /// <summary>
        /// Gets or sets the input file.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, HelpMessage = "The path to the file containing the terms to import or an XmlDocument object or XML string.", Position = 1)]
        [Alias("Xml")]
        public XmlDocumentPipeBind InputFile { get; set; }

        /// <summary>
        /// The end processing.
        /// </summary>
        protected override void InternalEndProcessing()
        {
            // Initialize XML serializer
            this.serializer = new XmlSerializer(typeof(UserProfileProperty));

            // Process XML
            var xml = this.InputFile.Read();
            var configFile = xml.ToXDocument();
            this.ProcessProfilePropertySettings(configFile);
        }

        private void ProcessProfilePropertySettings(XDocument configFile)
        {
            // Get all section nodes
            if (configFile.Root != null)
            {
                // Fetch site for context
                var siteUrl = configFile.Root.Attribute("Site").Value;
                using (var site = new SPSite(siteUrl))
                {
                    // Fetch service context and user profile manager objects
                    this.ResolveManagers(site);

                    var hideOtherProperties = configFile.Root.Attribute("HideOtherProperties") != null && bool.Parse(configFile.Root.Attribute("HideOtherProperties").Value);
                    foreach (var coreProperty in this.corePropertyManager.Where(x => !x.IsSection))
                    {
                        // Get deserialized configuration
                        var profilePropertyConfig =
                            configFile.Descendants("UserProperty")
                                .Select(x => (UserProfileProperty)this.serializer.Deserialize(x.CreateReader()))
                                .SingleOrDefault(x => x.GeneralSettings.Name.Equals(coreProperty.Name, StringComparison.OrdinalIgnoreCase));

                        if (profilePropertyConfig != null)
                        {
                            this.WriteVerbose(string.Format(CultureInfo.InvariantCulture, "Configuring profile property '{0}'...", profilePropertyConfig.GeneralSettings.Name));

                            // Get XML configuration node (need to test some attributes)
                            var profilePropertyConfigXmlNode = configFile.Descendants("GeneralSettings")
                                .Single(x => x.Attribute("Name").Value.Equals(coreProperty.Name, StringComparison.OrdinalIgnoreCase))
                                .Parent;

                            // Configure core property settings
                            this.ConfigureCoreProperty(coreProperty, profilePropertyConfig);

                            // Configure display settings
                            var profileTypeProperty = this.profileTypePropertyManager.GetPropertyByName(profilePropertyConfig.GeneralSettings.Name);
                            this.ConfigureProfileTypeProperty(profileTypeProperty, profilePropertyConfigXmlNode, profilePropertyConfig);

                            // Configure sub-type settings
                            var defaultProfileSubtype = this.profileSubTypeManager.GetProfileSubtype(ProfileSubtypeManager.GetDefaultProfileName(ProfileType.User));
                            var profileSubtypeProperty = defaultProfileSubtype.Properties.GetPropertyByName(profilePropertyConfig.GeneralSettings.Name);
                            this.ConfigureProfileSubtypeProperty(profileSubtypeProperty, profilePropertyConfigXmlNode, profilePropertyConfig);

                            // Change display order
                            if (profilePropertyConfig.GeneralSettings.Order > 0)
                            {
                                this.profileSubTypePropertyManager.SetDisplayOrderByPropertyName(profilePropertyConfig.GeneralSettings.Name, profilePropertyConfig.GeneralSettings.Order);
                                this.orderHasChanged = true;
                            }
                        }
                        else if (hideOtherProperties)
                        {
                            if (!string.IsNullOrEmpty(coreProperty.Name))
                            {
                                var hiddenPropertyConfig = new UserProfileProperty()
                                {
                                    GeneralSettings = new GeneralSettings()
                                    {
                                        Name = coreProperty.Name
                                    },
                                    DisplaySettings = new DisplaySettings()
                                    {
                                        IsUserEditable = false,
                                        IsVisibleOnEditor = false,
                                        IsVisibleOnViewer = false
                                    }
                                };

                                // Configure display settings
                                var profileTypeProperty = this.profileTypePropertyManager.GetPropertyByName(coreProperty.Name);
                                this.ConfigureProfileTypeProperty(profileTypeProperty, hiddenPropertyConfig);
                            }
                        }
                    }

                    if (this.orderHasChanged)
                    {
                        this.profileSubTypePropertyManager.CommitDisplayOrder();
                    }
                }
            }
        }

        private void ResolveManagers(SPSite site)
        {
            var serviceContext = SPServiceContext.GetContext(site);
            this.profileConfigManager = new UserProfileConfigManager(serviceContext);
            this.profilePropertyManager = this.profileConfigManager.ProfilePropertyManager;
            this.corePropertyManager = this.profilePropertyManager.GetCoreProperties();
            this.profileTypePropertyManager = this.profilePropertyManager.GetProfileTypeProperties(ProfileType.User);
            this.profileSubTypeManager = ProfileSubtypeManager.Get(serviceContext);
            this.profileSubTypePropertyManager = this.profilePropertyManager.GetProfileSubtypeProperties(ProfileSubtypeManager.GetDefaultProfileName(ProfileType.User));
        }

        private void ConfigureCoreProperty(CoreProperty coreProperty, UserProfileProperty profilePropertyConfig)
        {
            if (coreProperty != null)
            {
                // Set display name
                if (!string.IsNullOrEmpty(profilePropertyConfig.GeneralSettings.DisplayName))
                {
                    this.WriteVerbose(string.Format(CultureInfo.InvariantCulture, "Setting profile property '{0}' display name to '{1}'", profilePropertyConfig.GeneralSettings.Name, profilePropertyConfig.GeneralSettings.DisplayName));

                    coreProperty.DisplayName = profilePropertyConfig.GeneralSettings.DisplayName;
                }

                // Set length
                if (profilePropertyConfig.GeneralSettings.Length > 0)
                {
                    this.WriteVerbose(string.Format(CultureInfo.InvariantCulture, "Setting profile property '{0}' leght to '{1}'", profilePropertyConfig.GeneralSettings.Name, profilePropertyConfig.GeneralSettings.Length));

                    coreProperty.Length = profilePropertyConfig.GeneralSettings.Length;
                }

                // Set type
                if (!string.IsNullOrEmpty(profilePropertyConfig.GeneralSettings.Type))
                {
                    // Find type from string
                    var type =
                        this.profileConfigManager.GetPropertyDataTypes()
                            .Cast<PropertyDataType>()
                            .SingleOrDefault(x => x.Name.Equals(profilePropertyConfig.GeneralSettings.Type, StringComparison.OrdinalIgnoreCase));

                    if (type != null)
                    {
                        this.WriteVerbose(string.Format(CultureInfo.InvariantCulture, "Setting profile property '{0}' type to '{1}'", profilePropertyConfig.GeneralSettings.Name, type.Name));

                        coreProperty.Type = type.Name;
                    }
                }

                coreProperty.Commit();
            }
            else
            {
                this.WriteWarning(string.Format(CultureInfo.InvariantCulture, "Profile property with name '{0}' doesn't exist.  Please create it before executing this cmdlet.", profilePropertyConfig.GeneralSettings.Name));
            }
        }

        private void ConfigureProfileTypeProperty(ProfileTypeProperty profileTypeProperty, XElement profilePropertyConfigXmlNode, UserProfileProperty profilePropertyConfig)
        {
            if (profileTypeProperty != null)
            {
                var displaySettingsXmlNode = profilePropertyConfigXmlNode.Descendants("DisplaySettings").SingleOrDefault();
                if (displaySettingsXmlNode != null)
                {
                    if (displaySettingsXmlNode.Attribute("IsVisibleOnViewer") != null)
                    {
                        this.WriteVerbose(string.Format(CultureInfo.InvariantCulture, "Setting profile property '{0}' IsVisibleOnViewer to '{1}'", profilePropertyConfig.GeneralSettings.Name, profilePropertyConfig.DisplaySettings.IsVisibleOnViewer));
                        profileTypeProperty.IsVisibleOnViewer = profilePropertyConfig.DisplaySettings.IsVisibleOnViewer;
                    }

                    if (displaySettingsXmlNode.Attribute("IsVisibleOnEditor") != null)
                    {
                        this.WriteVerbose(string.Format(CultureInfo.InvariantCulture, "Setting profile property '{0}' IsVisibleOnEditor to '{1}'", profilePropertyConfig.GeneralSettings.Name, profilePropertyConfig.DisplaySettings.IsVisibleOnEditor));
                        profileTypeProperty.IsVisibleOnEditor = profilePropertyConfig.DisplaySettings.IsVisibleOnEditor;
                    }

                    if (displaySettingsXmlNode.Attribute("IsEventLog") != null)
                    {
                        this.WriteVerbose(string.Format(CultureInfo.InvariantCulture, "Setting profile property '{0}' IsEventLog to '{1}'", profilePropertyConfig.GeneralSettings.Name, profilePropertyConfig.DisplaySettings.IsEventLog));
                        profileTypeProperty.IsEventLog = profilePropertyConfig.DisplaySettings.IsEventLog;
                    }

                    profileTypeProperty.Commit();
                }
            }
            else
            {
                this.WriteWarning(string.Format(CultureInfo.InvariantCulture, "Profile type property with name '{0}' could not be found.", profilePropertyConfig.GeneralSettings.Name));
            }
        }

        private void ConfigureProfileTypeProperty(ProfileTypeProperty profileTypeProperty, UserProfileProperty profilePropertyConfig)
        {
            if (profileTypeProperty != null)
            {
                {
                    profileTypeProperty.IsVisibleOnViewer = profilePropertyConfig.DisplaySettings.IsVisibleOnViewer;
                    profileTypeProperty.IsVisibleOnEditor = profilePropertyConfig.DisplaySettings.IsVisibleOnEditor;
                    profileTypeProperty.IsEventLog = profilePropertyConfig.DisplaySettings.IsEventLog;
                    profileTypeProperty.Commit();
                }
            }
            else
            {
                this.WriteWarning(string.Format(CultureInfo.InvariantCulture, "Profile type property with name '{0}' could not be found.", profilePropertyConfig.GeneralSettings.Name));
            }
        }

        private void ConfigureProfileSubtypeProperty(ProfileSubtypeProperty profileSubtypeProperty, XElement profilePropertyConfigXmlNode, UserProfileProperty profilePropertyConfig)
        {
            if (profileSubtypeProperty != null)
            {
                var displaySettingsXmlNode = profilePropertyConfigXmlNode.Descendants("DisplaySettings").SingleOrDefault();
                if (displaySettingsXmlNode != null)
                {
                    if (displaySettingsXmlNode.Attribute("IsUserEditable") != null)
                    {
                        this.WriteVerbose(string.Format(CultureInfo.InvariantCulture, "Setting profile property '{0}' IsUserEditable to '{1}'", profilePropertyConfig.GeneralSettings.Name, profilePropertyConfig.DisplaySettings.IsUserEditable));

                        profileSubtypeProperty.IsUserEditable = profilePropertyConfig.DisplaySettings.IsUserEditable;
                    }

                    if (!string.IsNullOrEmpty(profilePropertyConfig.DisplaySettings.Privacy))
                    {
                        this.WriteVerbose(string.Format(CultureInfo.InvariantCulture, "Setting profile property '{0}' Privacy to '{1}'", profilePropertyConfig.GeneralSettings.Name, profilePropertyConfig.DisplaySettings.Privacy));
                    }

                    var privacy = profilePropertyConfig.DisplaySettings.Privacy.ToUpper(CultureInfo.InvariantCulture);
                    profileSubtypeProperty.DefaultPrivacy = (Privacy)Enum.Parse(typeof(Privacy), privacy, true);

                    try
                    {
                        if (!string.IsNullOrEmpty(profilePropertyConfig.DisplaySettings.PrivacyPolicy))
                        {
                            this.WriteVerbose(string.Format(CultureInfo.InvariantCulture, "Setting profile property '{0}' PrivacyPolicy to '{1}'", profilePropertyConfig.GeneralSettings.Name, profilePropertyConfig.DisplaySettings.PrivacyPolicy));
                        }

                        var privacyPolicy = profilePropertyConfig.DisplaySettings.PrivacyPolicy.ToUpper(CultureInfo.InvariantCulture);
                        profileSubtypeProperty.PrivacyPolicy = (PrivacyPolicy)Enum.Parse(typeof(PrivacyPolicy), privacyPolicy, true);
                    }
                    catch (UpdateSystemFieldException)
                    {
                        this.WriteWarning(string.Format(CultureInfo.InvariantCulture, "Cannot update system profile property '{0}' privacy policy.", profilePropertyConfig.GeneralSettings.Name));
                    }

                    profileSubtypeProperty.Commit();
                }
            }
            else
            {
                this.WriteWarning(string.Format(CultureInfo.InvariantCulture, "Profile subtype property with name '{0}' could not be found.", profilePropertyConfig.GeneralSettings.Name));
            }
        }
    }
}
