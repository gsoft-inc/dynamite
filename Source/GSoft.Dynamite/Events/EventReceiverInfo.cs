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
        /// <summary>
        /// Event Receiver Info (Content Type)
        /// </summary>
        /// <param name="contentType">Content type metadata</param>
        /// <param name="type">Type of receiver</param>
        public EventReceiverInfo(ContentTypeInfo contentType, SPEventReceiverType type)
        {
            this.ContentType = contentType;
            this.ReceiverType = type;
            this.EventOwner = EventReceiverOwner.ContentType;
        }

        /// <summary>
        /// Event Receiver Info (List)
        /// </summary>
        /// <param name="list">List metadata</param>
        /// <param name="type">Type of receiver</param>
        public EventReceiverInfo(ListInfo list, SPEventReceiverType type)
        {
            this.List = list;
            this.EventOwner = EventReceiverOwner.List;
            this.ReceiverType = type;
        }

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
    }
}
