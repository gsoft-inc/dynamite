using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSoft.Dynamite.Events
{
    /// <summary>
    /// Types of event receivers
    /// </summary>
    public enum EventReceiverOwner
    {
        /// <summary>
        /// Receiver for all instances of items with a particular content type
        /// </summary>
        ContentType,

        /// <summary>
        /// Receiver for all items in a list
        /// </summary>
        List
    }
}
