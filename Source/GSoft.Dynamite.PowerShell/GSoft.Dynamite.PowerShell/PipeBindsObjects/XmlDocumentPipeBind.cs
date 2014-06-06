using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Xml;
using Microsoft.SharePoint.PowerShell;

namespace GSoft.Dynamite.PowerShell.PipeBindsObjects
{
    /// <summary>
    /// Original class from Gary Lapointe Cmdlets
    /// http://blog.falchionconsulting.com/index.php/downloads/
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
    public class XmlDocumentPipeBind : SPCmdletPipeBind<XmlDocument>
    {
        private string xml;

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlDocumentPipeBind"/> class.
        /// </summary>
        /// <param name="instance">
        /// The instance.
        /// </param>
        public XmlDocumentPipeBind(XmlDocument instance)
            : base(instance)
        {
            this.xml = instance.OuterXml;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlDocumentPipeBind"/> class.
        /// </summary>
        /// <param name="inputString">
        /// The input string.
        /// </param>
        /// <exception cref="SPCmdletPipeBindException">
        /// Thrown if the input string is not a valid XML file.
        /// </exception>
        public XmlDocumentPipeBind(string inputString)
        {
            var xmlDocument = new XmlDocument();
            try
            {
                if (File.Exists(inputString))
                {
                    xmlDocument.Load(inputString);
                }
                else
                {
                    xmlDocument.LoadXml(inputString);
                }
            }
            catch
            {
                throw new SPCmdletPipeBindException("The input string is not a valid XML file.");
            }

            this.xml = xmlDocument.OuterXml;
        }

        /// <summary>
        /// The read.
        /// </summary>
        /// <returns>
        /// The <see cref="XmlDocument"/>.
        /// </returns>
        public override XmlDocument Read()
        {
            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(this.xml);

            return xmlDocument;
        }

        /// <summary>
        /// The discover.
        /// </summary>
        /// <param name="instance">
        /// The instance.
        /// </param>
        protected override void Discover(XmlDocument instance)
        {
            this.xml = instance.OuterXml;
        }
    }
}
