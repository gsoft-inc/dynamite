using System;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.Utils
{
    /// <summary>
    /// Scope in which the item events are disabled.
    /// </summary>
    public class DisabledItemEventsScope : SPItemEventReceiver, IDisposable
    {
        private readonly bool _oldValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="DisabledItemEventsScope"/> class.
        /// </summary>
        public DisabledItemEventsScope()
        {
            this._oldValue = this.EventFiringEnabled;
            this.EventFiringEnabled = false;
        }

        #region IDisposable Members

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.EventFiringEnabled = this._oldValue;
        }

        #endregion
    }
}
