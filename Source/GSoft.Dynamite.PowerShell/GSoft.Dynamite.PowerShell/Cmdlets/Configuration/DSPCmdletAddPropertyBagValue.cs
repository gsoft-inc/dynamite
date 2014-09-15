using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using System.Xml.Linq;
using System.Xml.Serialization;
using Autofac;
using GSoft.Dynamite.Configuration;
using GSoft.Dynamite.PowerShell.Extensions;
using GSoft.Dynamite.PowerShell.PipeBindsObjects;
using GSoft.Dynamite.PowerShell.Unity;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
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
        private XmlSerializer serializer;

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
            this.serializer = new XmlSerializer(typeof(PropertyBagValue));

            // Process XML
            var xml = this.InputFile.Read();
            var configFile = xml.ToXDocument();
            this.ProcessPropertyBagValues(configFile);
        }

        private void ProcessPropertyBagValues(XDocument configFile)
        {
            // Get all web application nodes
            var webApplicationNodes = configFile.Descendants("WebApplication").Select(x => x);
            foreach (var webApplicationNode in webApplicationNodes)
            {
                // For each web application, create and configure the property bag values
                this.SetWebApplicationPropertyBagValue(webApplicationNode);
            }

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
                        using (var childScope = PowerShellContainer.BeginLifetimeScope(web))
                        {
                            var propertyBagHelper = childScope.Resolve<PropertyBagHelper>();
                            var propertyBagValues = webNode.Descendants("PropertyBagValue").Select(x => (PropertyBagValue)this.serializer.Deserialize(x.CreateReader())).ToList();
                            propertyBagHelper.SetWebValues(web, propertyBagValues);
                        }
                    }
                }
            }
        }

        [Obsolete("To be replaced with PropertyBagHelper method when the BeginWebApplication Lifetime scope will work")]
        private void SetWebApplicationPropertyBagValue(XElement webApplicationNode)
        {
            var webApplicationUrl = webApplicationNode.Attribute("Url").Value;
            var webApplication = SPWebApplication.Lookup(new Uri(webApplicationUrl));
            var propertyBagValues =
                webApplicationNode.Descendants("PropertyBagValue")
                    .Select(x => (PropertyBagValue)this.serializer.Deserialize(x.CreateReader()));

            foreach (var propertyBagValue in propertyBagValues)
            {
                // Add value to root web property bag
                if (webApplication.Properties.ContainsKey(propertyBagValue.Key) && propertyBagValue.Overwrite)
                {
                    this.WriteWarning(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            "Overwriting property bag '{0}' with value '{1}' to web application '{2}'",
                            propertyBagValue.Key,
                            propertyBagValue.Value,
                            webApplicationUrl));

                    webApplication.Properties[propertyBagValue.Key] = propertyBagValue.Value;
                }
                else if (!webApplication.Properties.ContainsKey(propertyBagValue.Key))
                {
                    this.WriteVerbose(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            "Adding property bag '{0}' with value '{1}' to web application '{2}'",
                            propertyBagValue.Key,
                            propertyBagValue.Value,
                            webApplicationUrl));

                    webApplication.Properties.Add(propertyBagValue.Key, propertyBagValue.Value);
                }

                webApplication.Update();
            }
        }
    }
}
