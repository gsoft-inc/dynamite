using System.Globalization;
using log4net;

namespace GSoft.Dynamite.Sharepoint2013.Logging.Log4Net
{
    /// <summary>
    /// An ILogger implementation wrapped around a Log4Net ILog instance.
    /// </summary>
    internal sealed class Log4NetLogger : ILogger
    {
        #region Fields

        private readonly ILog _log;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Log4NetLogger"/> class. 
        /// Creates a new instance of the Log4NetLogger class, wrapped around the specified ILog instance.
        /// </summary>
        /// <param name="log">
        /// The ILog instance to wrap.
        /// </param>
        internal Log4NetLogger(ILog log)
        {
            this._log = log;
        }

        #endregion

        #region ILogger Members

        /// <summary>
        /// Returns <c>true</c> if debug-level logging is enabled.
        /// </summary>
        public bool IsDebugEnabled
        {
            get
            {
                return this._log.IsDebugEnabled;
            }
        }

        /// <summary>
        /// Output the message at the Debug level.
        /// </summary>
        /// <param name="message">
        /// The message to output.
        /// </param>
        public void Debug(object message)
        {
            this._log.Debug(message);
        }

        /// <summary>
        /// Output the formatted message at the Debug level.
        /// </summary>
        /// <param name="format">
        /// The format to use.
        /// </param>
        /// <param name="args">
        /// The arguments to pass to the formatter.
        /// </param>
        public void Debug(string format, params object[] args)
        {
            this._log.DebugFormat(CultureInfo.InvariantCulture, format, args);
        }

        /// <summary>
        /// Output the message at the Error level.
        /// </summary>
        /// <param name="message">
        /// The message to output.
        /// </param>
        public void Error(object message)
        {
            this._log.Error(message);
        }

        /// <summary>
        /// Output the formatted message at the Error level.
        /// </summary>
        /// <param name="format">
        /// The format to use.
        /// </param>
        /// <param name="args">
        /// The arguments to pass to the formatter.
        /// </param>
        public void Error(string format, params object[] args)
        {
            this._log.ErrorFormat(CultureInfo.InvariantCulture, format, args);
        }

        /// <summary>
        /// Output the message at the Fatal level.
        /// </summary>
        /// <param name="message">
        /// The message to output.
        /// </param>
        public void Fatal(object message)
        {
            this._log.Fatal(message);
        }

        /// <summary>
        /// Output the formatted message at the Fatal level.
        /// </summary>
        /// <param name="format">
        /// The format to use.
        /// </param>
        /// <param name="args">
        /// The arguments to pass to the formatter.
        /// </param>
        public void Fatal(string format, params object[] args)
        {
            this._log.FatalFormat(CultureInfo.InvariantCulture, format, args);
        }

        /// <summary>
        /// Output the message at the Info level.
        /// </summary>
        /// <param name="message">
        /// The message to output.
        /// </param>
        public void Info(object message)
        {
            this._log.Info(message);
        }

        /// <summary>
        /// Output the formatted message at the Info level.
        /// </summary>
        /// <param name="format">
        /// The format to use.
        /// </param>
        /// <param name="args">
        /// The arguments to pass to the formatter.
        /// </param>
        public void Info(string format, params object[] args)
        {
            this._log.InfoFormat(CultureInfo.InvariantCulture, format, args);
        }

        /// <summary>
        /// Output the message at the Warn level.
        /// </summary>
        /// <param name="message">
        /// The message to output.
        /// </param>
        public void Warn(object message)
        {
            this._log.Warn(message);
        }

        /// <summary>
        /// Output the formatted message at the Warn level.
        /// </summary>
        /// <param name="format">
        /// The format to use.
        /// </param>
        /// <param name="args">
        /// The arguments to pass to the formatter.
        /// </param>
        public void Warn(string format, params object[] args)
        {
            this._log.WarnFormat(CultureInfo.InvariantCulture, format, args);
        }

        #endregion
    }
}