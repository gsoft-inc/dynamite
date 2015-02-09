using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using GSoft.Dynamite.ContentTypes;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.Events
{
    /// <summary>
    /// Helper class the manage event receivers.
    /// </summary>
    public class EventReceiverHelper : IEventReceiverHelper
    {
        /// <summary>
        /// Initializes a new <see cref="EventReceiverHelper"/> instance.
        /// </summary>
        public EventReceiverHelper()
        {
        }

        /// <summary>
        /// Does the event receiver definition exist in the collection?
        /// </summary>
        /// <param name="collection">The event receiver definition collection.</param>
        /// <param name="type">The event receiver type.</param>
        /// <param name="assemblyFullName">Full name of the assembly.</param>
        /// <param name="classFullName">Full name of the class.</param>
        /// <returns>
        ///   <c>True</c> if the event receiver definition is found, else <c>False</c>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">For any null parameter.</exception>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of statics is discouraged - this favors more flexibility and consistency with dependency injection.")]
        public bool EventReceiverDefinitionExist(SPEventReceiverDefinitionCollection collection, SPEventReceiverType type, string assemblyFullName, string classFullName)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }

            if (string.IsNullOrEmpty(assemblyFullName))
            {
                throw new ArgumentNullException("assemblyFullName");
            }

            if (string.IsNullOrEmpty(classFullName))
            {
                throw new ArgumentNullException("classFullName");
            }

            // If there is nothing in the collection we don't even need to check.
            if (collection.Count <= 0)
            {
                return false;
            }

            // Get the event receiver if it exists.
            SPEventReceiverDefinition eventReceiver = this.GetEventReceiverDefinition(collection, type, assemblyFullName, classFullName);
            return eventReceiver != null;
        }

        /// <summary>
        /// Gets the event receiver definition.
        /// </summary>
        /// <param name="collection">The event receiver definition collection.</param>
        /// <param name="type">The event receiver type.</param>
        /// <param name="assemblyFullName">Full name of the assembly.</param>
        /// <param name="classFullName">Full name of the class.</param>
        /// <returns>The event receiver definition if found, else null.</returns>
        /// <exception cref="System.ArgumentNullException">For any null parameter.</exception>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of statics is discouraged - this favors more flexibility and consistency with dependency injection.")]
        public SPEventReceiverDefinition GetEventReceiverDefinition(SPEventReceiverDefinitionCollection collection, SPEventReceiverType type, string assemblyFullName, string classFullName)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }

            if (string.IsNullOrEmpty(assemblyFullName))
            {
                throw new ArgumentNullException("assemblyFullName");
            }

            if (string.IsNullOrEmpty(classFullName))
            {
                throw new ArgumentNullException("classFullName");
            }

            foreach (SPEventReceiverDefinition eventReceiver in collection)
            {
                bool isCorrectType = eventReceiver.Type == type;
                bool isCorrectAssembly = string.Compare(eventReceiver.Assembly, assemblyFullName, StringComparison.OrdinalIgnoreCase) == 0;
                bool isCorrectClass = string.Compare(eventReceiver.Class, classFullName, StringComparison.OrdinalIgnoreCase) == 0;

                if (isCorrectType && isCorrectAssembly && isCorrectClass)
                {
                    return eventReceiver;
                }
            }

            return null;
        }

        /// <summary>
        /// Add an event receiver
        /// </summary>
        /// <param name="site">The site</param>
        /// <param name="eventReceiver">The event receiver definition</param>
        public void AddEventReceiverDefinition(SPSite site, EventReceiverInfo eventReceiver)
        {
            // Content Types
            if (eventReceiver.EventOwner == EventReceiverOwner.ContentType)
            {
                var contentType = site.RootWeb.ContentTypes[new SPContentTypeId(eventReceiver.ContentType.ContentTypeId)];
                if (contentType != null)
                {
                    this.AddEventReceiverDefinition(contentType, eventReceiver.ReceiverType, eventReceiver.AssemblyName, eventReceiver.ClassName, eventReceiver.SynchronizationType);
                }
            }  
        }

        /// <summary>
        /// Remove an event receiver
        /// </summary>
        /// <param name="site">The site</param>
        /// <param name="eventReceiver">The event receiver definition</param>
        public void DeleteEventReceiverDefinition(SPSite site, EventReceiverInfo eventReceiver)
        {
            // Content Types
            if (eventReceiver.EventOwner == EventReceiverOwner.ContentType)
            {
                var contentType = site.RootWeb.ContentTypes[new SPContentTypeId(eventReceiver.ContentType.ContentTypeId)];
                if (contentType != null)
                {
                    this.DeleteEventReceiverDefinition(contentType, eventReceiver.ReceiverType, eventReceiver.ClassName);
                }
            }
        }

        /// <summary>
        /// Remove the event receiver definition for the content type.
        /// </summary>
        /// <param name="contentType">The content type.</param>
        /// <param name="type">The receiver type.</param>
        /// <param name="className">Name of the class.</param>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of statics is discouraged - this favors more flexibility and consistency with dependency injection.")]
        public void DeleteEventReceiverDefinition(SPContentType contentType, SPEventReceiverType type, string className)
        {
            var eventReceiverDefinition = contentType.EventReceivers.Cast<SPEventReceiverDefinition>().FirstOrDefault(x => (x.Class == className) && (x.Type == type));

            // If definition isn't already defined, add it to the content type
            if (eventReceiverDefinition != null)
            {
                var eventToDelete = contentType.EventReceivers.Cast<SPEventReceiverDefinition>().Where(eventReceiver => eventReceiver.Type == eventReceiverDefinition.Type).ToList();
                eventToDelete.ForEach(c => c.Delete());
                contentType.Update(true);
            }
        }

        /// <summary>
        /// Adds the event receiver definition to the content type.
        /// </summary>
        /// <param name="contentType">The content type.</param>
        /// <param name="type">The receiver type.</param>
        /// <param name="assemblyName">Name of the assembly.</param>
        /// <param name="className">Name of the class.</param>
        /// <param name="syncType">The synchronization type</param>
        /// <returns>The event receiver definition</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of statics is discouraged - this favors more flexibility and consistency with dependency injection.")]
        private SPEventReceiverDefinition AddEventReceiverDefinition(SPContentType contentType, SPEventReceiverType type, string assemblyName, string className, SPEventReceiverSynchronization syncType)
        {
            SPEventReceiverDefinition eventReceiverDefinition = null;

            var classType = Type.GetType(string.Format(CultureInfo.InvariantCulture, "{0}, {1}", className, assemblyName));
            if (classType != null)
            {
                var assembly = Assembly.GetAssembly(classType);
                eventReceiverDefinition = this.AddEventReceiverDefinition(contentType, type, assembly, className, syncType);
            }

            return eventReceiverDefinition;
        }

        /// <summary>
        /// Adds the event receiver definition to the content type.
        /// </summary>
        /// <param name="contentType">The content type.</param>
        /// <param name="type">The receiver type.</param>
        /// <param name="assembly">The assembly.</param>
        /// <param name="className">Name of the class.</param>
        /// <param name="syncType">The synchronization type</param>
        /// <returns>The event receiver definition</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of statics is discouraged - this favors more flexibility and consistency with dependency injection.")]
        private SPEventReceiverDefinition AddEventReceiverDefinition(SPContentType contentType, SPEventReceiverType type, Assembly assembly, string className, SPEventReceiverSynchronization syncType)
        {
            SPEventReceiverDefinition eventReceiverDefinition = null;

            var isAlreadyDefined = contentType.EventReceivers.Cast<SPEventReceiverDefinition>()
                .Any(x => (x.Class == className) && (x.Type == type));

            // If definition isn't already defined, add it to the content type
            if (!isAlreadyDefined)
            {
                eventReceiverDefinition = contentType.EventReceivers.Add();
                eventReceiverDefinition.Type = type;
                eventReceiverDefinition.Assembly = assembly.FullName;
                eventReceiverDefinition.Synchronization = syncType;
                eventReceiverDefinition.Class = className;
                eventReceiverDefinition.Update();
                contentType.Update(true);
            }

            return eventReceiverDefinition;
        }
    }
}
