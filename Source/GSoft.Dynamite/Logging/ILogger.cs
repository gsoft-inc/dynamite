using System.Diagnostics.CodeAnalysis;

namespace GSoft.Dynamite.Sharepoint2013.Logging
{
    /// <summary>
    /// Defines the standard logging interface.
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Returns TRUE if debug-level logging is enabled.
        /// </summary>
        bool IsDebugEnabled { get; }

        /// <summary>
        /// Output the message at the Debug level.
        /// </summary>
        /// <param name="message">The message to output.</param>
        void Debug(object message);

        /// <summary>
        /// Output the formatted message at the Debug level.
        /// </summary>
        /// <param name="format">The format to use.</param>
        /// <param name="args">The arguments to pass to the formatter.</param>
        void Debug(string format, params object[] args);

        /// <summary>
        /// Output the message at the Info level.
        /// </summary>
        /// <param name="message">The message to output.</param>
        void Info(object message);

        /// <summary>
        /// Output the formatted message at the Info level.
        /// </summary>
        /// <param name="format">The format to use.</param>
        /// <param name="args">The arguments to pass to the formatter.</param>
        void Info(string format, params object[] args);

        /// <summary>
        /// Output the message at the Warn level.
        /// </summary>
        /// <param name="message">The message to output.</param>
        void Warn(object message);

        /// <summary>
        /// Output the formatted message at the Warn level.
        /// </summary>
        /// <param name="format">The format to use.</param>
        /// <param name="args">The arguments to pass to the formatter.</param>
        void Warn(string format, params object[] args);

        /// <summary>
        /// Output the message at the Error level.
        /// </summary>
        /// <param name="message">The message to output.</param>
        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", Justification = "Error is valid in this case.")]
        void Error(object message);

        /// <summary>
        /// Output the formatted message at the Error level.
        /// </summary>
        /// <param name="format">The format to use.</param>
        /// <param name="args">The arguments to pass to the formatter.</param>
        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", Justification = "Error is valid in this case.")]
        void Error(string format, params object[] args);

        /// <summary>
        /// Output the message at the Fatal level.
        /// </summary>
        /// <param name="message">The message to output.</param>
        void Fatal(object message);

        /// <summary>
        /// Output the formatted message at the Fatal level.
        /// </summary>
        /// <param name="format">The format to use.</param>
        /// <param name="args">The arguments to pass to the formatter.</param>
        void Fatal(string format, params object[] args);
    }
}
