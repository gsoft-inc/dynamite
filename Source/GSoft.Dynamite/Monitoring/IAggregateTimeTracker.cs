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
    /// TODO: Update summary.
    /// </summary>
    public interface IAggregateTimeTracker
    {
        TimeSpan GetAggregateTimeSpentForKey(string key);

        void AddTimeSpanToAggregateTimeSpentForKey(string key, TimeSpan timeSpan);

        TimeTrackerScope BeginTimeTrackerScope(string key);

    }
}
