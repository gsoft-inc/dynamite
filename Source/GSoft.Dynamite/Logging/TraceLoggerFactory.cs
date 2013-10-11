using System;

namespace GSoft.Dynamite.Sharepoint.Logging
{
    /// <summary>
    /// The factory for Trace loggers.
    /// </summary>
    public class TraceLoggerFactory : ILoggerFactory
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TraceLoggerFactory"/> class.
        /// </summary>
        /// <param name="categoryName">Name of the category.</param>
        /// <param name="isDebugEnabled">if set to <c>true</c> sets if debug is enabled.</param>
        public TraceLoggerFactory(string categoryName, bool isDebugEnabled)
        {
            this.CategoryName = categoryName;
            this.IsDebugEnabled = isDebugEnabled;
        }

        /// <summary>
        /// Gets or sets the name of the category.
        /// </summary>
        public string CategoryName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is debug enabled.
        /// </summary>
        public bool IsDebugEnabled { get; set; }

        /// <summary>
        /// Create the ILogger for a particular type.
        /// </summary>
        /// <param name="type">The type to create the logger for.</param>
        /// <returns>
        /// The ILogger instance.
        /// </returns>
        public ILogger Create(Type type)
        {
            return new TraceLogger(type.FullName, this.CategoryName, this.IsDebugEnabled);
        }

        /// <summary>
        /// Create the ILogger for a particular name.
        /// </summary>
        /// <param name="name">The name to create the logger for.</param>
        /// <returns>
        /// The ILogger instance.
        /// </returns>
        public ILogger Create(string name)
        {
            return new TraceLogger(name, this.CategoryName, this.IsDebugEnabled);
        }
    }
}
