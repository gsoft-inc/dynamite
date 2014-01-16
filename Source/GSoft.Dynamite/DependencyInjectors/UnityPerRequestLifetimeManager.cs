using System;
using System.Web;
using Microsoft.Practices.Unity;

namespace GSoft.Dynamite.Unity
{
    /// <summary>
    /// Pre-request lifetime manager. Only one instance of the class is used per HttpRequest.
    /// </summary>
    public class PerRequestLifetimeManager : LifetimeManager, IDisposable
    {
        private readonly object key = new object();

        /// <summary>
        /// Retrieve the lifetime manager controlled object
        /// </summary>
        /// <returns>The object is an HttpContext exists, null otherwise.</returns>
        public override object GetValue()
        {
            if (HttpContext.Current != null && HttpContext.Current.Items.Contains(this.key))
            {
                return HttpContext.Current.Items[this.key];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Clear the lifetime manager controlled object
        /// </summary>
        public override void RemoveValue()
        {
            if (HttpContext.Current != null)
            {
                HttpContext.Current.Items.Remove(this.key);
            }
        }

        /// <summary>
        /// Adds the object under lifetime manager control
        /// </summary>
        /// <param name="newValue">New value</param>
        public override void SetValue(object newValue)
        {
            if (HttpContext.Current != null)
            {
                HttpContext.Current.Items[this.key] = newValue;
            }
        }

        /// <summary>
        /// Disposal removes the object from lifetime manager control
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposal removes the object from lifetime manager control
        /// </summary>
        /// <param name="cleanupManagedResources">Whether both managed and native resources should be freed</param>
        protected virtual void Dispose(bool cleanupManagedResources)
        {
            this.RemoveValue();
        }
    }
}
