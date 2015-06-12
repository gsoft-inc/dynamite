using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.ReusableContent
{
    /// <summary>
    /// Contract on the Helper for the Reusable Content
    /// </summary>
    public interface IReusableContentHelper
    {
        /// <summary>
        /// Method to ensure (create if not exist) and update a reusable content in a specific site.
        /// </summary>
        /// <param name="site">The Site Collection to ensure the reusablec content</param>
        /// <param name="reusableContents">The information on the reusable contents to ensure</param>
        void EnsureReusableContent(SPSite site, IList<ReusableContentInfo> reusableContents);
    }
}
