using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using System.Xml.Linq;
using System.Xml.Serialization;
using GSoft.Dynamite.PowerShell.Cmdlets.Configuration.Entities;
using GSoft.Dynamite.PowerShell.Extensions;
using GSoft.Dynamite.PowerShell.PipeBindsObjects;
using Microsoft.SharePoint;
using Microsoft.SharePoint.PowerShell;

namespace GSoft.Dynamite.PowerShell.Cmdlets.Configuration
{
    /// <summary>
    /// Cmdlet for property bag value creation
    /// </summary>
    [Cmdlet(VerbsCommon.Add, "DSPPropertyBagValue")]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
    // ReSharper disable once InconsistentNaming
    public class DSPCmdletAddPropertyBagValue : SPCmdlet
    {
        private XmlSerializer _serializer;
        
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
        protected override void InternalEndProcessing()
        {
            // Initialize XML serializer
            this._serializer = new XmlSerializer(typeof(PropertyBagValue));

            // Process XML
            var xml = this.InputFile.Read();
            var configFile = xml.ToXDocument(); 
            this.ProcessPropertyBagValues(configFile);
        }

        private void ProcessPropertyBagValues(XDocument configFile)
        {
            // Get all site nodes
            var webNodes = configFile.Descendants("Web").Select(x => x);
            foreach (var webNode in webNodes)
            {
                // For each site, create and configure the property bag values
                var webUrl = webNode.Attribute("Url").Value;
                using (var site = new SPSite(webUrl))
                {
                    using (var web = site.OpenWeb())
                    {
                        var propertyBagValues = webNode.Descendants("PropertyBagValue").Select(x => (PropertyBagValue)this._serializer.Deserialize(x.CreateReader()));
                        foreach (var propertyBagValue in propertyBagValues)
                        {
                            // Add value to root web property bag
                            if (web.AllProperties.ContainsKey(propertyBagValue.Key) && propertyBagValue.Overwrite)
                            {
                                this.WriteWarning(
                                    string.Format(
                                    CultureInfo.InvariantCulture,
                                    "Overwriting property bag '{0}' with value '{1}' to web '{2}'",
                                    propertyBagValue.Key,
                                    propertyBagValue.Value,
                                    webUrl));

                                web.AllProperties[propertyBagValue.Key] = propertyBagValue.Value;
                            }
                            else if (!web.AllProperties.ContainsKey(propertyBagValue.Key))
                            {
                                this.WriteVerbose(
                                    string.Format(
                                    CultureInfo.InvariantCulture,
                                    "Adding property bag '{0}' with value '{1}' to web '{2}'",
                                    propertyBagValue.Key,
                                    propertyBagValue.Value,
                                    webUrl));

                                web.AllProperties.Add(propertyBagValue.Key, propertyBagValue.Value);
                            }

                            // Add property bag key to indexed property keys
                            if (!web.IndexedPropertyKeys.Contains(propertyBagValue.Key) && propertyBagValue.Indexed)
                            {
                                this.WriteVerbose(
                                    string.Format(
                                    CultureInfo.InvariantCulture,
                                    "Setting property bag '{0}' to be indexable by search on web '{1}'",
                                    propertyBagValue.Key,
                                    webUrl));

                                web.IndexedPropertyKeys.Add(propertyBagValue.Key);
                            }
                        }

                        web.Update();
                    }
                }
            }
        }
    }
}
