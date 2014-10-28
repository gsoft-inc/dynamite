using System;
using System.Diagnostics.CodeAnalysis;
using GSoft.Dynamite.Definitions;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.Helpers
{
    /// <summary>
    /// Helper class the manage event receivers.
    /// </summary>
    public class EventReceiverHelper : IEventReceiverHelper
    {
        private readonly ContentTypeHelper contentTypeHelper;

        public EventReceiverHelper(ContentTypeHelper contentTypeHelper)
        {
            this.contentTypeHelper = contentTypeHelper;
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
            if (eventReceiver.EventOwner == EventReceiverInfo.EventReceiverOwner.ContentType)
            {
                var contentType = this.contentTypeHelper.EnsureContentType(site.RootWeb.AvailableContentTypes, eventReceiver.ContentType);

                if (contentType != null)
                {
                    this.contentTypeHelper.AddEventReceiverDefinition(contentType, eventReceiver.ReceiverType, eventReceiver.AssemblyName, eventReceiver.ClassName);
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
            if (eventReceiver.EventOwner == EventReceiverInfo.EventReceiverOwner.ContentType)
            {
                var contentType = this.contentTypeHelper.EnsureContentType(site.RootWeb.AvailableContentTypes, eventReceiver.ContentType);

                if (contentType != null)
                {
                    this.contentTypeHelper.DeleteEventReceiverDefinition(contentType, eventReceiver.ReceiverType, eventReceiver.ClassName);
                }
            }
        }

    }
}
