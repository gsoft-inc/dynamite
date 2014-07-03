using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace GSoft.Dynamite.PowerShell.Extensions
{
    /// <summary>
    /// The xmlDocument document extensions.
    /// </summary>
    public static class XmlDocumentExtensions
    {
        /// <summary>
        /// The to x document.
        /// </summary>
        /// <param name="document">
        /// The document.
        /// </param>
        /// <returns>
        /// The <see cref="XDocument"/>.
        /// </returns>
        public static XDocument ToXDocument(this XmlDocument document)
        {
            return document.ToXDocument(LoadOptions.None);
        }

        /// <summary>
        /// The to x document.
        /// </summary>
        /// <param name="document">
        /// The document.
        /// </param>
        /// <param name="options">
        /// The options.
        /// </param>
        /// <returns>
        /// The <see cref="XDocument"/>.
        /// </returns>
        public static XDocument ToXDocument(this XmlDocument document, LoadOptions options)
        {
            using (XmlNodeReader reader = new XmlNodeReader(document))
            {
                return XDocument.Load(reader, options);
            }
        }
    }
}
