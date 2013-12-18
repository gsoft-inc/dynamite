using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.SharePoint.PowerShell;

namespace GSoft.Dynamite.PowerShell.PipeBindsObjects
{
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
            XmlDocument xml = new XmlDocument();
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
            XmlDocument xml = new XmlDocument();
            xml.LoadXml(_xml);
            return xml;
        }
    }
}
