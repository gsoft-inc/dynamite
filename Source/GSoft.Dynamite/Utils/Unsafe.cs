using System;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.Utils
{
    /// <summary>
    /// Class to be able to automatically allow unsafe update and reset the value with
    /// using(new Unsafe(web)){}
    /// </summary>
    public sealed class Unsafe : IDisposable
    {
        private bool _originalAllowUnsafeUpdates;
        private SPWeb _web = null;

        /// <summary>
        /// Constructor where we store the original value of AllowUnsafeUpdates
        /// </summary>
        /// <param name="originalWeb">The WebSite to make unsafe updates</param>
        public Unsafe(SPWeb originalWeb)
        {
            this._web = originalWeb;
            this._originalAllowUnsafeUpdates = originalWeb.AllowUnsafeUpdates;
            originalWeb.AllowUnsafeUpdates = true;
        }

        /// <summary>
        /// Destructor where we reset to original value of the AllowUnsafeUpdates
        /// </summary>
        public void Dispose()
        {
            this._web.AllowUnsafeUpdates = this._originalAllowUnsafeUpdates;
        }
    }
}
