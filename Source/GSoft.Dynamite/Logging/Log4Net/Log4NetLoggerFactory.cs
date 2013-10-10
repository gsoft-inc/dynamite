using System;
using log4net;
using log4net.Config;

namespace GSoft.Dynamite.Sharepoint2013.Logging.Log4Net
{
    /// <summary>
    /// Factory to create Log4NetLogger instances.
    /// </summary>
    public sealed class Log4NetLoggerFactory : ILoggerFactory, IDisposable
    {
        /// <summary>
        /// Creates a new instance of the Log4NetLoggerFactory class.
        /// </summary>
        public Log4NetLoggerFactory()
        {
            // load App/Web.config settings
           XmlConfigurator.Configure();
        }

        /// <summary>
        /// Create the ILogger for a particular type.
        /// </summary>
        /// <param name="type">The type to create the logger for.</param>
        /// <returns>The ILogger instance.</returns>
        public ILogger Create(Type type)
        {
            return new Log4NetLogger(LogManager.GetLogger(type));
        }

        /// <summary>
        /// Create the ILogger for a particular name.
        /// </summary>
        /// <param name="name">The name to create the logger for.</param>
        /// <returns>The ILogger instance.</returns>
        public ILogger Create(string name)
        {
            return new Log4NetLogger(LogManager.GetLogger(name));
        }

        /// <summary>
        /// Dispose this object.
        /// </summary>
        public void Dispose()
        {
            LogManager.Shutdown();
            GC.SuppressFinalize(this);
        }
    }
}
