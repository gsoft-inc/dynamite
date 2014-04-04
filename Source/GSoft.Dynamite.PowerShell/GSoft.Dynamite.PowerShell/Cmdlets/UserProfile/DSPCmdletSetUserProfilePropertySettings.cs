using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using System.Xml.Linq;
using System.Xml.Serialization;
using GSoft.Dynamite.PowerShell.Cmdlets.Configuration.Entities;
using GSoft.Dynamite.PowerShell.Cmdlets.UserProfile.Entities;
using GSoft.Dynamite.PowerShell.Extensions;
using GSoft.Dynamite.PowerShell.PipeBindsObjects;
using Microsoft.Office.Server.UserProfiles;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;

namespace GSoft.Dynamite.PowerShell.Cmdlets.UserProfile
{
    /// <summary>
    /// Cmdlet for user profile 
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "DSPUserProfilePropertySettings")]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
    // ReSharper disable once InconsistentNaming
    public class DSPCmdletSetUserProfilePropertySettings : Cmdlet
    {
        private XmlSerializer _serializer;
        private UserProfileConfigManager _profileConfigManager;
        private ProfilePropertyManager _profilePropertyManager;
        private CorePropertyManager _corePropertyManager;
        private ProfileTypePropertyManager _profileTypePropertyManager;
        private ProfileSubtypeManager _profileSubTypeManager;
        private ProfileSubtypePropertyManager _profileSubTypePropertyManager;
        private bool _orderHasChanged;

        /// <summary>
        /// Gets or sets the input file.
        /// </summary>
        [Parameter(Mandatory = true,
            ValueFromPipeline = true,
            HelpMessage = "The path to the file containing the terms to import or an XmlDocument object or XML string.",
            Position = 1)]
        [Alias("Xml")]
        public XmlDocumentPipeBind InputFile { get; set; }

        /// <summary>
        /// The end processing.
        /// </summary>
        protected override void EndProcessing()
        {
            // Initialize XML serializer
            this._serializer = new XmlSerializer(typeof(UserProfileProperty));

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
                    foreach (var coreProperty in this._corePropertyManager.Where(x => !x.IsSection))
                    {
                        // Get deserialized configuration
                        var profilePropertyConfig =
                            configFile.Descendants("UserProperty")
                                .Select(x => (UserProfileProperty)this._serializer.Deserialize(x.CreateReader()))
                                .SingleOrDefault(x => x.GeneralSettings.Name.Equals(coreProperty.Name, StringComparison.OrdinalIgnoreCase));

                        if (profilePropertyConfig != null)
                        {
                            this.WriteVerbose(
                                string.Format(
                                    CultureInfo.InvariantCulture,
                                    "Configuring profile property '{0}'...",
                                    profilePropertyConfig.GeneralSettings.Name));

                            // Get XML configuration node (need to test some attributes)
                            var profilePropertyConfigXmlNode = configFile.Descendants("GeneralSettings")
                                .Single(x => x.Attribute("Name").Value.Equals(coreProperty.Name, StringComparison.OrdinalIgnoreCase))
                                .Parent;

                            // Configure core property settings
                            this.ConfigureCoreProperty(coreProperty, profilePropertyConfig);

                            // Configure display settings
                            var profileTypeProperty = this._profileTypePropertyManager.GetPropertyByName(profilePropertyConfig.GeneralSettings.Name);
                            this.ConfigureProfileTypeProperty(profileTypeProperty, profilePropertyConfigXmlNode, profilePropertyConfig);

                            // Configure sub-type settings
                            var defaultProfileSubtype =
                                this._profileSubTypeManager.GetProfileSubtype(
                                    ProfileSubtypeManager.GetDefaultProfileName(ProfileType.User));
                            var profileSubtypeProperty =
                                defaultProfileSubtype.Properties.GetPropertyByName(profilePropertyConfig.GeneralSettings.Name);
                            this.ConfigureProfileSubtypeProperty(
                                profileSubtypeProperty,
                                profilePropertyConfigXmlNode,
                                profilePropertyConfig); 

                            // Change display order
                            if (profilePropertyConfig.GeneralSettings.Order > 0)
                            {
                                this._profileSubTypePropertyManager.SetDisplayOrderByPropertyName(
                                    profilePropertyConfig.GeneralSettings.Name,
                                    profilePropertyConfig.GeneralSettings.Order);
                                this._orderHasChanged = true;
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
                                var profileTypeProperty = this._profileTypePropertyManager.GetPropertyByName(coreProperty.Name);
                                this.ConfigureProfileTypeProperty(profileTypeProperty, hiddenPropertyConfig); 
                            }
                        }
                    }

                    if (this._orderHasChanged)
                    {
                        this._profileSubTypePropertyManager.CommitDisplayOrder(); 
                    }
                }
            }
        }

        private void ResolveManagers(SPSite site)
        {
            var serviceContext = SPServiceContext.GetContext(site);
            this._profileConfigManager = new UserProfileConfigManager(serviceContext);
            this._profilePropertyManager = this._profileConfigManager.ProfilePropertyManager;
            this._corePropertyManager = this._profilePropertyManager.GetCoreProperties();
            this._profileTypePropertyManager = this._profilePropertyManager.GetProfileTypeProperties(ProfileType.User);
            this._profileSubTypeManager = ProfileSubtypeManager.Get(serviceContext);
            this._profileSubTypePropertyManager =
                this._profilePropertyManager.GetProfileSubtypeProperties(
                    ProfileSubtypeManager.GetDefaultProfileName(ProfileType.User));
        }

        private void ConfigureCoreProperty(CoreProperty coreProperty, UserProfileProperty profilePropertyConfig)
        {
            if (coreProperty != null)
            {
                // Set display name
                if (!string.IsNullOrEmpty(profilePropertyConfig.GeneralSettings.DisplayName))
                {
                    this.WriteVerbose(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            "Setting profile property '{0}' display name to '{1}'",
                            profilePropertyConfig.GeneralSettings.Name,
                            profilePropertyConfig.GeneralSettings.DisplayName));

                    coreProperty.DisplayName = profilePropertyConfig.GeneralSettings.DisplayName;
                }

                // Set length
                if (profilePropertyConfig.GeneralSettings.Length > 0)
                {
                    this.WriteVerbose(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            "Setting profile property '{0}' leght to '{1}'",
                            profilePropertyConfig.GeneralSettings.Name,
                            profilePropertyConfig.GeneralSettings.Length));

                    coreProperty.Length = profilePropertyConfig.GeneralSettings.Length;
                }

                // Set type
                if (!string.IsNullOrEmpty(profilePropertyConfig.GeneralSettings.Type))
                {
                    // Find type from string
                    var type =
                        this._profileConfigManager.GetPropertyDataTypes()
                            .Cast<PropertyDataType>()
                            .SingleOrDefault(x => x.Name.Equals(profilePropertyConfig.GeneralSettings.Type, StringComparison.OrdinalIgnoreCase));

                    if (type != null)
                    {
                        this.WriteVerbose(
                            string.Format(
                                CultureInfo.InvariantCulture,
                                "Setting profile property '{0}' type to '{1}'",
                                profilePropertyConfig.GeneralSettings.Name,
                                type.Name));

                        coreProperty.Type = type.Name;
                    }
                }

                coreProperty.Commit();
            }
            else
            {
                this.WriteWarning(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "Profile property with name '{0}' doesn't exist.  Please create it before executing this cmdlet.",
                        profilePropertyConfig.GeneralSettings.Name));
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
                        this.WriteVerbose(
                            string.Format(
                                CultureInfo.InvariantCulture,
                                "Setting profile property '{0}' IsVisibleOnViewer to '{1}'",
                                profilePropertyConfig.GeneralSettings.Name,
                                profilePropertyConfig.DisplaySettings.IsVisibleOnViewer));

                        profileTypeProperty.IsVisibleOnViewer = profilePropertyConfig.DisplaySettings.IsVisibleOnViewer;
                    }

                    if (displaySettingsXmlNode.Attribute("IsVisibleOnEditor") != null)
                    {
                        this.WriteVerbose(
                            string.Format(
                                CultureInfo.InvariantCulture,
                                "Setting profile property '{0}' IsVisibleOnEditor to '{1}'",
                                profilePropertyConfig.GeneralSettings.Name,
                                profilePropertyConfig.DisplaySettings.IsVisibleOnEditor));

                        profileTypeProperty.IsVisibleOnEditor = profilePropertyConfig.DisplaySettings.IsVisibleOnEditor;
                    }

                    if (displaySettingsXmlNode.Attribute("IsEventLog") != null)
                    {
                        this.WriteVerbose(
                            string.Format(
                                CultureInfo.InvariantCulture,
                                "Setting profile property '{0}' IsEventLog to '{1}'",
                                profilePropertyConfig.GeneralSettings.Name,
                                profilePropertyConfig.DisplaySettings.IsEventLog));

                        profileTypeProperty.IsEventLog = profilePropertyConfig.DisplaySettings.IsEventLog;
                    }

                    profileTypeProperty.Commit();
                }
            }
            else
            {
                this.WriteWarning(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "Profile type property with name '{0}' could not be found.",
                        profilePropertyConfig.GeneralSettings.Name));
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
                this.WriteWarning(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "Profile type property with name '{0}' could not be found.",
                        profilePropertyConfig.GeneralSettings.Name));
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
                        this.WriteVerbose(
                            string.Format(
                                CultureInfo.InvariantCulture,
                                "Setting profile property '{0}' IsUserEditable to '{1}'",
                                profilePropertyConfig.GeneralSettings.Name,
                                profilePropertyConfig.DisplaySettings.IsUserEditable));

                        profileSubtypeProperty.IsUserEditable = profilePropertyConfig.DisplaySettings.IsUserEditable;
                    }

                    if (!string.IsNullOrEmpty(profilePropertyConfig.DisplaySettings.Privacy))
                    {
                        this.WriteVerbose(
                            string.Format(
                                CultureInfo.InvariantCulture,
                                "Setting profile property '{0}' Privacy to '{1}'",
                                profilePropertyConfig.GeneralSettings.Name,
                                profilePropertyConfig.DisplaySettings.Privacy)); 
                    }

                    switch (profilePropertyConfig.DisplaySettings.Privacy.ToUpper(CultureInfo.InvariantCulture))
                    {
                        case "PUBLIC":
                        profileSubtypeProperty.DefaultPrivacy = Privacy.Public;
                        break;

                        case "PRIVATE":
                        profileSubtypeProperty.DefaultPrivacy = Privacy.Private;
                        break;

                        case "ORGANIZATION":
                        profileSubtypeProperty.DefaultPrivacy = Privacy.Organization;
                        break;

                        case "NOTSET":
                        profileSubtypeProperty.DefaultPrivacy = Privacy.NotSet;
                        break;

                        case "MANAGER":
                        profileSubtypeProperty.DefaultPrivacy = Privacy.Manager;
                        break;

                        case "CONTACTS":
                        profileSubtypeProperty.DefaultPrivacy = Privacy.Contacts;
                        break;
                    }

                    try
                    {
                        if (!string.IsNullOrEmpty(profilePropertyConfig.DisplaySettings.PrivacyPolicy))
                        {
                            this.WriteVerbose(
                                string.Format(
                                    CultureInfo.InvariantCulture,
                                    "Setting profile property '{0}' PrivacyPolicy to '{1}'",
                                    profilePropertyConfig.GeneralSettings.Name,
                                    profilePropertyConfig.DisplaySettings.PrivacyPolicy));
                        }

                        switch (profilePropertyConfig.DisplaySettings.PrivacyPolicy.ToUpper(CultureInfo.InvariantCulture))
                        {
                            case "MANDATORY":
                                profileSubtypeProperty.PrivacyPolicy = PrivacyPolicy.Mandatory;
                                break;

                            case "OPTIN":
                                profileSubtypeProperty.PrivacyPolicy = PrivacyPolicy.OptIn;
                                break;

                            case "OPTOUT":
                                profileSubtypeProperty.PrivacyPolicy = PrivacyPolicy.OptOut;
                                break;

                            case "DISABLED":
                                profileSubtypeProperty.PrivacyPolicy = PrivacyPolicy.Disabled;
                                break;
                        }
                    }
                    catch (UpdateSystemFieldException)
                    {
                        this.WriteWarning(
                            string.Format(
                            CultureInfo.InvariantCulture,
                            "Cannot update system profile property '{0}' privacy policy.", 
                            profilePropertyConfig.GeneralSettings.Name));
                    }

                    profileSubtypeProperty.Commit();
                }
            }
            else
            {
                this.WriteWarning(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "Profile subtype property with name '{0}' could not be found.",
                        profilePropertyConfig.GeneralSettings.Name));
            }
        }
    }
}
