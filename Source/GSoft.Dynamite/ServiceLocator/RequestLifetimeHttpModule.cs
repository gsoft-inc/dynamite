using System;
using System.Collections.Generic;
using System.Web;

namespace GSoft.Dynamite.ServiceLocator
{
    /// <summary>
    /// <c>Autofac-related</c> HttpModule that takes care of disposing per-request lifetimes
    /// at the end of each HTTP request.
    /// </summary>
    public class RequestLifetimeHttpModule : IHttpModule
    {
        private static IDictionary<string, ILifetimeScopeProvider> allLifetimeScopeProviders = new Dictionary<string, ILifetimeScopeProvider>();

        /// <summary>
        /// Initializes a module and prepares it to handle requests.
        /// </summary>
        /// <param name="context">An <see cref="T:System.Web.HttpApplication"/> that provides access to the 
        /// methods, properties, and events common to all application objects within an ASP.NET application</param>
        public void Init(HttpApplication context)
        {
            context.EndRequest += OnEndRequest;
        }

        /// <summary>
        /// Disposes of the resources (other than memory) used by the module that implements <see cref="T:System.Web.IHttpModule"/>.
        /// </summary>
        public void Dispose()
        {
        }

        /// <summary>
        /// Add a new Lifetime scope provider.
        /// </summary>
        /// <param name="uniqueContainerKey">The unique container key</param>
        /// <param name="requestLifetimeScopeProvider">The provider to add in the dictionary</param>
        public static void AddRequestLifetimeScopeProvider(string uniqueContainerKey, SPRequestLifetimeScopeProvider requestLifetimeScopeProvider)
        {
            if (string.IsNullOrEmpty(uniqueContainerKey))
            {
                throw new ArgumentNullException("uniqueContainerKey");
            }

            if (requestLifetimeScopeProvider == null)
            {
                throw new ArgumentNullException("lifetimeScopeProvider");
            }

            // Add to dictionary of (there can be a different per-request lifetime provider for each container in the AppDomain)
            allLifetimeScopeProviders[uniqueContainerKey] = requestLifetimeScopeProvider;
        }

        /// <summary>
        /// Event handler to end a Lifetime scope at the end of a http request
        /// </summary>
        /// <param name="sender">The event sender</param>
        /// <param name="e">Arguments related to the event</param>
        public static void OnEndRequest(object sender, EventArgs e)
        {
            foreach (ILifetimeScopeProvider provider in allLifetimeScopeProviders.Values)
            {
                // End all per-request lifetimes across all containers on the current request
                provider.EndLifetimeScope();
            }
        }
    }
}
