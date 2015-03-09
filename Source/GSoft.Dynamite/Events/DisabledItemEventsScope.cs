using System;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.Events
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
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <param name="cleanUpBothNativeAndManaged">This parameter is ignored. EventFiringEnabled is always set back to is preceding value.</param>
        protected virtual void Dispose(bool cleanUpBothNativeAndManaged)
        {
            this.EventFiringEnabled = this._oldValue;
        }

        #endregion
    }
}
