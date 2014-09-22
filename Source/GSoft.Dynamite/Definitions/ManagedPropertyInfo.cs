using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSoft.Dynamite.Definitions
{
    /// <summary>
    /// Definition for a managed property
    /// </summary>
    public class ManagedPropertyInfo
    {
        /// <summary>
        /// Initializes a new ManagedPropertyInfo
        /// </summary>
        /// <param name="name">The name of the managed property</param>
        public ManagedPropertyInfo(string name)
        {
            this.Name = name;
        }

        /// <summary>
        /// Name of the managed property
        /// </summary>
        public string Name { get; set; }
    }
}
