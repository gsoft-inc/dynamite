using System.IO;
using System.Xml;
using Microsoft.SharePoint.PowerShell;

namespace GSoft.Dynamite.PowerShell.PipeBindsObjects
{
    /// <summary>
    /// Original class from Gary Lapointe Cmdlets
    /// http://blog.falchionconsulting.com/index.php/downloads/
    /// </summary>
    public class XmlDocumentPipeBind : SPCmdletPipeBind<XmlDocument>
    {
        private string _xml;

        public XmlDocumentPipeBind(XmlDocument instance)
            : base(instance)
        {
            _xml = instance.OuterXml;
        }

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
            _xml = xml.OuterXml;
        }



        protected override void Discover(XmlDocument instance)
        {
            _xml = instance.OuterXml;
        }

        public override XmlDocument Read()
        {
            var xml = new XmlDocument();
            xml.LoadXml(_xml);
            return xml;
        }
    }
}
