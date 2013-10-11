using System;

namespace GSoft.Dynamite.Sharepoint2013.Logging
{
    /// <summary>
    /// The standard interface for creating ILogger objects.
    /// </summary>
    public interface ILoggerFactory
    {
        /// <summary>
        /// Create the ILogger for a particular type.
        /// </summary>
        /// <param name="type">The type to create the logger for.</param>
        /// <returns>The ILogger instance.</returns>
        ILogger Create(Type type);

        /// <summary>
        /// Create the ILogger for a particular name.
        /// </summary>
        /// <param name="name">The name to create the logger for.</param>
        /// <returns>The ILogger instance.</returns>
        ILogger Create(string name);
    }
}
