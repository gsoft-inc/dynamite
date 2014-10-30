using System;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.Utils
{
    /// <summary>
    /// Wrap your code with this handler to swallow and log all exceptions
    /// </summary>
    /// <remarks>
    /// This is meant as a presentation utility to prevent errors in Web Parts from making whole pages explode.
    /// Do not use this utility in internal or service-level code, because catching all exception types is 
    /// usually considered a bad practice.
    /// </remarks>
    public interface ICatchAllExceptionHandler
    {
        /// <summary>
        /// Calls the void-returning method and swallows (+ logs) all exceptions types
        /// </summary>
        /// <param name="web">The context's web.</param>
        /// <param name="methodToInvoke">The delegate to invoke.</param>
        void Execute(SPWeb web, Action methodToInvoke);
    }
}
