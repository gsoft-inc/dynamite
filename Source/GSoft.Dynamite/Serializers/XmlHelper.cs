using System.Xml;
using GSoft.Dynamite.Logging;

namespace GSoft.Dynamite.Serializers
{
    /// <summary>
    /// Helper to work with Xml
    /// </summary>
    public class XmlHelper : IXmlHelper
    {
        private readonly ILogger logger;

        /// <summary>
        /// The default constructor
        /// </summary>
        /// <param name="logger">The logger</param>
        public XmlHelper(ILogger logger)
        {
            this.logger = logger;
        }

        /// <summary>
        /// Method to create a XmlElement from a string. We put the string in the InnerText because the SharePoint API only reads that property
        /// </summary>
        /// <param name="xml">The xml content</param>
        /// <returns>An XmlElement if the xml was parsed.</returns>
        public XmlElement CreateXmlElementInnerTextFromString(string xml)
        {
            XmlElement element = null;

            var xmlDoc = new XmlDocument();
            try
            {
                element = xmlDoc.CreateElement("HtmlElement");
                element.InnerText = xml;
            }
            catch (XmlException exception)
            {
                this.logger.Error("The following Xml can't be parsed. {0}. Stack: {1}", xml, exception.StackTrace);
            }

            return element;
        }
    }
}
