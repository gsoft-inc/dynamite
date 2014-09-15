// -----------------------------------------------------------------------
// <copyright file="IAggregateTimeTracker.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace GSoft.Dynamite.Monitoring
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Aggregate time tracking utility
    /// </summary>
    public interface IAggregateTimeTracker
    {
        /// <summary>
        /// Fetches the total time spent in code segments that
        /// were timed using the specified key
        /// </summary>
        /// <param name="key">The key that was used to track time</param>
        /// <returns>The sum time span</returns>
        TimeSpan GetAggregateTimeSpentForKey(string key);

        /// <summary>
        /// Adds time to the total for the specified key
        /// </summary>
        /// <param name="key">The key of the timings aggregate</param>
        /// <param name="timeSpan">The time spent that must be added</param>
        void AddTimeSpanToAggregateTimeSpentForKey(string key, TimeSpan timeSpan);

        /// <summary>
        /// Starts a time tracking scope
        /// </summary>
        /// <param name="key">The aggregate timings key</param>
        /// <returns>The disposable timing scope</returns>
        TimeTrackerScope BeginTimeTrackerScope(string key);
    }
}
