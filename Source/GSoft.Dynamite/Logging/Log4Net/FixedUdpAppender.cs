using System;
using System.Globalization;
using System.Net.Sockets;
using log4net.Appender;
using log4net.Core;

namespace GSoft.Dynamite.Sharepoint2013.Logging.Log4Net
{
    /// <summary>
    /// A fixed version of the <see cref="UdpAppender"/> class, adding support for IPv6 addresses.
    /// </summary>
    public class FixedUdpAppender : UdpAppender
    {
        #region Methods

        /// <summary>
        /// Initialize the client connection.
        /// </summary>
        protected override void InitializeClientConnection()
        {
            try
            {
                if (this.LocalPort == 0)
                {
                    this.Client = new UdpClient(this.RemoteAddress.AddressFamily);
                }
                else
                {
                    this.Client = new UdpClient(this.LocalPort, this.RemoteAddress.AddressFamily);
                }
            }
            catch (Exception exception)
            {
                this.ErrorHandler.Error("Could not initialize the UdpClient connection on port " + this.LocalPort.ToString(NumberFormatInfo.InvariantInfo) + ".", exception, ErrorCode.GenericFailure);
                this.Client = null;
            }
        }

        #endregion
    }
}