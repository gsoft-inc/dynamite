using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSoft.Dynamite.Fields.Types
{
    /// <summary>
    /// Selection modes supported by <see cref="UserFieldInfo"/>
    /// </summary>
    public enum UserFieldSelectionMode
    {
        /// <summary>
        /// Default setting, only individuals can be chosen through the people picker
        /// </summary>
        PeopleOnly,

        /// <summary>
        /// Both people and groups are available through the people picker
        /// </summary>
        PeopleAndGroups        
    }
}
