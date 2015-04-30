using GSoft.Dynamite.ContentTypes;
using GSoft.Dynamite.Lists;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.Events
{
    /// <summary>
    /// Easily serializable representation of event receiver metadata
    /// </summary>
    public class EventReceiverInfo
    {
        #region Constructors

        /// <summary>
        /// Default constructor for serialization purposes
        /// </summary>
        public EventReceiverInfo()
        {
        }

        /// <summary>
        /// Event Receiver Info (Content Type)
        /// </summary>
        /// <param name="contentType">The content type</param>
        /// <param name="type">The event receiver type</param>
        public EventReceiverInfo(ContentTypeInfo contentType, SPEventReceiverType type)
            : this(contentType, type, SPEventReceiverSynchronization.Default)
        {
        }

        /// <summary>
        /// Event Receiver Info (Content Type)
        /// </summary>
        /// <param name="contentType">The content type</param>
        /// <param name="type">The event receiver type</param>
        /// <param name="syncType">The synchronization type</param>
        public EventReceiverInfo(ContentTypeInfo contentType, SPEventReceiverType type, SPEventReceiverSynchronization syncType)
            : this(contentType, type, syncType, string.Empty, string.Empty)
        {
        }

        /// <summary>
        /// Event Receiver Info (Content Type)
        /// </summary>
        /// <param name="contentType">The content type</param>
        /// <param name="type">The event receiver type</param>
        /// <param name="syncType">The synchronization type</param>
        /// <param name="assemblyName">The full name of the Assembly</param>
        /// <param name="className">The fullname of the Type/Class </param>
        public EventReceiverInfo(ContentTypeInfo contentType, SPEventReceiverType type, SPEventReceiverSynchronization syncType, string assemblyName, string className)
        {
            this.ContentType = contentType;
            this.ReceiverType = type;
            this.SynchronizationType = syncType;
            this.AssemblyName = assemblyName;
            this.ClassName = className;
        }

        /// <summary>
        /// Event Receiver Info (List)
        /// </summary>
        /// <param name="list">The list</param>
        /// <param name="type">The event receiver type</param>
        public EventReceiverInfo(ListInfo list, SPEventReceiverType type)
            : this(list, type, SPEventReceiverSynchronization.Default)
        {
        }

        /// <summary>
        /// Event Receiver Info (List)
        /// </summary>
        /// <param name="list">The list</param>
        /// <param name="type">The event receiver type</param>
        /// <param name="syncType">The synchronization type</param>
        public EventReceiverInfo(ListInfo list, SPEventReceiverType type, SPEventReceiverSynchronization syncType)
            : this(list, type, syncType, string.Empty, string.Empty)
        {
        }

        /// <summary>
        /// Event Receiver Info (List)
        /// </summary>
        /// <param name="list">The list</param>
        /// <param name="type">The event receiver type</param>
        /// <param name="syncType">The synchronization type</param>
        /// <param name="assemblyName">The full name of the Assembly</param>
        /// <param name="className">The fullname of the Type/Class </param>
        public EventReceiverInfo(ListInfo list, SPEventReceiverType type, SPEventReceiverSynchronization syncType, string assemblyName, string className)
        {
            this.List = list;
            this.ReceiverType = type;
            this.SynchronizationType = syncType;
            this.AssemblyName = assemblyName;
            this.ClassName = className;
        }

        #endregion Constructors

        /// <summary>
        /// The associated content type
        /// </summary>
        public ContentTypeInfo ContentType { get; set; }

        /// <summary>
        /// The associated list
        /// </summary>
        public ListInfo List { get; set; }

        /// <summary>
        /// The receiver type
        /// </summary>
        public SPEventReceiverType ReceiverType { get; set; }

        /// <summary>
        /// The assembly name
        /// </summary>
        public string AssemblyName { get; set; }

        /// <summary>
        /// The class name
        /// </summary>
        public string ClassName { get; set; }

        /// <summary>
        /// Synchronization type for the event receiver
        /// </summary>
        public SPEventReceiverSynchronization SynchronizationType { get; set; }
    }
}
