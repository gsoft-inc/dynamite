namespace GSoft.Dynamite.Events
{
    using System.Diagnostics.CodeAnalysis;

    using Microsoft.SharePoint;

    public interface IEventReceiverHelper
    {
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
        bool EventReceiverDefinitionExist(SPEventReceiverDefinitionCollection collection, SPEventReceiverType type, string assemblyFullName, string classFullName);

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
        SPEventReceiverDefinition GetEventReceiverDefinition(SPEventReceiverDefinitionCollection collection, SPEventReceiverType type, string assemblyFullName, string classFullName);


        /// <summary>
        /// Add an event receiver
        /// </summary>
        /// <param name="site">The site</param>
        /// <param name="eventReceiver">The event receiver definition</param>
        void AddEventReceiverDefinition(SPSite site, EventReceiverInfo eventReceiver);

        /// <summary>
        /// Remove an event receiver
        /// </summary>
        /// <param name="site">The site</param>
        /// <param name="eventReceiver">The event receiver definition</param>
        void DeleteEventReceiverDefinition(SPSite site, EventReceiverInfo eventReceiver);
    }
}