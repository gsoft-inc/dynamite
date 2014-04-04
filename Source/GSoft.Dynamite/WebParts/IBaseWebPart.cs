// -----------------------------------------------------------------------
// <copyright file="IBaseWebPart.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace GSoft.Dynamite.WebParts
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Text;
    using System.Web.UI.WebControls.WebParts;

    /// <summary>
    /// Basic web part properties, mirroring the contents of a <see cref="System.Web.UI.WebControls.WebParts.WebPart"/>
    /// </summary>
    public interface IBaseWebPart
    {
        /// <summary>
        /// Web part title
        /// </summary>
        string Title { get; set; }

        /// <summary>
        /// We part storage key string
        /// </summary>
        string ID { get; set; }

        /// <summary>
        /// Chrome type
        /// </summary>
        PartChromeType ChromeType { get; set; }

        /// <summary>
        /// Security trimming (e.g. audience targeting) filter string
        /// </summary>
        /// <remarks>
        /// See formatting here: http://msdn.microsoft.com/en-us/library/office/ee906649(v=office.14).aspx
        /// </remarks>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
        string AuthorizationFilter { get; set; }
    }
}
