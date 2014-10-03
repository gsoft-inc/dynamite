using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSoft.Dynamite.Security
{
    using Microsoft.SharePoint;

    /// <summary>
    /// The Security mapping interface
    /// </summary>
    public interface ISecurityMappings
    {
        /// <summary>
        /// The get principal role pairs.
        /// </summary>
        /// <param name="web">The current web</param>
        /// <returns>
        /// The collection of <see cref="IObjectRoles"/>.
        /// </returns>
        ICollection<IObjectRoles> PermissionsToApply(SPWeb web);
    }
}
