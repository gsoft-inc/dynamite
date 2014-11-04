using GSoft.Dynamite.ContentTypes;
using GSoft.Dynamite.Lists;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.Events
{
    public class EventReceiverInfo
    {
        public enum EventReceiverOwner
        {
            ContentType,
            List
        }

        /// <summary>
        /// Event Receiver Info (Content Type)
        /// </summary>
        /// <param name="contentType"></param>
        /// <param name="type"></param>
        public EventReceiverInfo(ContentTypeInfo contentType, SPEventReceiverType type)
            : this (contentType, type, SPEventReceiverSynchronization.Default)
        {
        }

        /// <summary>
        /// Event Receiver Info (Content Type)
        /// </summary>
        /// <param name="contentType"></param>
        /// <param name="type"></param>
        /// <param name="syncType">Synchronization type</param>
        public EventReceiverInfo(ContentTypeInfo contentType, SPEventReceiverType type, SPEventReceiverSynchronization syncType)
        {
            this.ContentType = contentType;
            this.ReceiverType = type;
            this.EventOwner = EventReceiverOwner.ContentType;
            this.SynchronizationType = syncType;
        }

        /// <summary>
        /// Event Receiver Info (List)
        /// </summary>
        /// <param name="list"></param>
        /// <param name="type"></param>
        public EventReceiverInfo(ListInfo list, SPEventReceiverType type) : this(list, type, SPEventReceiverSynchronization.Default)
        {
        }

        /// <summary>
        /// Event Receiver Info (List)
        /// </summary>
        /// <param name="list"></param>
        /// <param name="type"></param>
        /// <param name="syncType">Synchronization type</param>
        public EventReceiverInfo(ListInfo list, SPEventReceiverType type, SPEventReceiverSynchronization syncType)
        {
            this.List = list;
            this.EventOwner = EventReceiverOwner.List;
            this.ReceiverType = type;
            this.SynchronizationType = syncType;
        }

        /// <summary>
        /// The associated content type
        /// </summary>
        public ContentTypeInfo ContentType { get; private set; }

        /// <summary>
        /// The associated list
        /// </summary>
        public ListInfo List { get; private set; }

        /// <summary>
        /// The receiver type
        /// </summary>
        public SPEventReceiverType ReceiverType { get; private set; }

        /// <summary>
        /// The assembly name
        /// </summary>
        public string AssemblyName { get; set; }

        /// <summary>
        /// The class name
        /// </summary>
        public string ClassName { get;  set; }

        /// <summary>
        /// The owner of the event receiver
        /// </summary>
        public EventReceiverOwner EventOwner { get; private set; }

        /// <summary>
        /// Synchronization type for the event receiver
        /// </summary>
        public SPEventReceiverSynchronization SynchronizationType { get; set; }
    }
}
