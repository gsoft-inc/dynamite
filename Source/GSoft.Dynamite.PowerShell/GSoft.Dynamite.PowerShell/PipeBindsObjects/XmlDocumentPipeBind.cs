using System.IO;
using System.Xml;

using Microsoft.SharePoint.PowerShell;

namespace GSoft.Dynamite.PowerShell.PipeBindsObjects
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Original class from Gary Lapointe Cmdlets
    /// http://blog.falchionconsulting.com/index.php/downloads/
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
    public class XmlDocumentPipeBind : SPCmdletPipeBind<XmlDocument>
    {
        private string _xml;

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlDocumentPipeBind"/> class.
        /// </summary>
        /// <param name="instance">
        /// The instance.
        /// </param>
        public XmlDocumentPipeBind(XmlDocument instance)
            : base(instance)
        {
            this._xml = instance.OuterXml;
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
            var xml = new XmlDocument();
            try
            {
                if (File.Exists(inputString))
                {
                    xml.Load(inputString);
                }
                else
                {
                    xml.LoadXml(inputString);
                }
            }
            catch
            {
                throw new SPCmdletPipeBindException("The input string is not a valid XML file.");
            }

            this._xml = xml.OuterXml;
        }

        /// <summary>
        /// The read.
        /// </summary>
        /// <returns>
        /// The <see cref="XmlDocument"/>.
        /// </returns>
        public override XmlDocument Read()
        {
            var xml = new XmlDocument();
            xml.LoadXml(this._xml);
            return xml;
        }

        /// <summary>
        /// The discover.
        /// </summary>
        /// <param name="instance">
        /// The instance.
        /// </param>
        protected override void Discover(XmlDocument instance)
        {
            this._xml = instance.OuterXml;
        }
    }
}
