namespace GSoft.Dynamite.Sharepoint.Logging
{
    /// <summary>
    /// A logger that logs to SharePoint's ULS.
    /// </summary>
    public class TraceLogger : ILogger
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TraceLogger"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="categoryName">Name of the category.</param>
        /// <param name="isDebugEnabled">if set to <c>true</c> [is debug enabled].</param>
        public TraceLogger(string name, string categoryName, bool isDebugEnabled)
        {
            this.Name = name;
            this.CategoryName = categoryName;
            this.IsDebugEnabled = isDebugEnabled;
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Returns TRUE if debug-level logging is enabled.
        /// </summary>
        public bool IsDebugEnabled { get; private set; }

        /// <summary>
        /// Gets the name of the category.
        /// </summary>
        public string CategoryName { get; private set; }

        /// <summary>
        /// Output the message at the Debug level.
        /// </summary>
        /// <param name="message">The message to output.</param>
        public void Debug(object message)
        {
            if (this.IsDebugEnabled)
            {
                this.Debug("{0}", message);
            }
        }

        /// <summary>
        /// Output the formatted message at the Debug level.
        /// </summary>
        /// <param name="format">The format to use.</param>
        /// <param name="args">The arguments to pass to the formatter.</param>
        public void Debug(string format, params object[] args)
        {
            if (this.IsDebugEnabled)
            {
                this.InnerLog(TraceSeverity.Verbose, format, args);
            }
        }

        /// <summary>
        /// Output the message at the Info level.
        /// </summary>
        /// <param name="message">The message to output.</param>
        public void Info(object message)
        {
            this.Info("{0}", message);
        }

        /// <summary>
        /// Output the formatted message at the Info level.
        /// </summary>
        /// <param name="format">The format to use.</param>
        /// <param name="args">The arguments to pass to the formatter.</param>
        public void Info(string format, params object[] args)
        {
            this.InnerLog(TraceSeverity.Medium, format, args);
        }

        /// <summary>
        /// Output the message at the Warn level.
        /// </summary>
        /// <param name="message">The message to output.</param>
        public void Warn(object message)
        {
            this.Warn("{0}", message);
        }

        /// <summary>
        /// Output the formatted message at the Warn level.
        /// </summary>
        /// <param name="format">The format to use.</param>
        /// <param name="args">The arguments to pass to the formatter.</param>
        public void Warn(string format, params object[] args)
        {
            this.InnerLog(TraceSeverity.High, format, args);
        }

        /// <summary>
        /// Output the message at the Error level.
        /// </summary>
        /// <param name="message">The message to output.</param>
        public void Error(object message)
        {
            this.Error("{0}", message);
        }

        /// <summary>
        /// Output the formatted message at the Error level.
        /// </summary>
        /// <param name="format">The format to use.</param>
        /// <param name="args">The arguments to pass to the formatter.</param>
        public void Error(string format, params object[] args)
        {
            this.InnerLog(TraceSeverity.Unexpected, format, args);
        }

        /// <summary>
        /// Output the message at the Fatal level.
        /// </summary>
        /// <param name="message">The message to output.</param>
        public void Fatal(object message)
        {
            this.Fatal("{0}", message);
        }

        /// <summary>
        /// Output the formatted message at the Fatal level.
        /// </summary>
        /// <param name="format">The format to use.</param>
        /// <param name="args">The arguments to pass to the formatter.</param>
        public void Fatal(string format, params object[] args)
        {
            this.InnerLog(TraceSeverity.Unexpected, format, args);
        }

        /// <summary>
        /// Logs to the ULS.
        /// </summary>
        /// <param name="traceSeverity">The trace severity.</param>
        /// <param name="message">The message.</param>
        /// <param name="args">The message arguments.</param>
        protected virtual void InnerLog(TraceSeverity traceSeverity, string message, params object[] args)
        {
            SPDiagnosticsService.Local.WriteTrace(
                0,
                new SPDiagnosticsCategory(this.CategoryName, TraceSeverity.Medium, EventSeverity.Information),
                traceSeverity,
                this.Name + " - " + message, 
                args);
        }
    }
}
