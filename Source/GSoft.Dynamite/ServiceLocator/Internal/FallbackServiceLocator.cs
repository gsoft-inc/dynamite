using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GSoft.Dynamite.ServiceLocator.AddOn;

namespace GSoft.Dynamite.ServiceLocator.Internal
{
    /// <summary>
    /// Service Locator that will be used by default if you fail to define a <see cref="ISharePointServiceLocatorAccess"/>
    /// of your own (the usual convention is that you provide such an implementation within an assembly that matches the pattern
    /// *.ServiceLocator.DLL.
    /// By default, this fallback service locator will load all available Dynamite modules.
    /// </summary>
    internal class FallbackServiceLocator : ISharePointServiceLocatorAccessor
    {
        private const string AppName = "GSoft.Dynamite";

        /// <summary>
        /// By default, the service locator instance will load all Dynamite registration modules available in the GAC
        /// (i.e. all those modules located within assemblies that match the pattern "GSoft.Dynamite*.DLL")
        /// </summary>
        private static readonly ISharePointServiceLocator InnerFallbackServiceLocator = new SharePointServiceLocator("GSoft.Dynamite");

        /// <summary>
        /// Service locator instance
        /// </summary>
        public ISharePointServiceLocator ServiceLocatorInstance
        {
            get 
            { 
                return InnerFallbackServiceLocator; 
            }
        }
    }
}
