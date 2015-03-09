namespace GSoft.Dynamite.Serializers
{
    using System.Diagnostics.CodeAnalysis;
    using System.Xml;

    /// <summary>
    /// Helper to work with Xml
    /// </summary>
    public interface IXmlHelper
    {
        /// <summary>
        /// Method to create a XmlElement from a string. We put the string in the InnerText because the SharePoint API only reads that property
        /// </summary>
        /// <param name="xml">The xml content</param>
        /// <returns>An XmlElement if the xml was parsed.</returns>
        [SuppressMessage("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes", MessageId = "System.Xml.XmlNode", Justification = "SharePoint API requires XmlElement parameters from time to time.")]
        XmlElement CreateXmlElementInnerTextFromString(string xml);
    }
}