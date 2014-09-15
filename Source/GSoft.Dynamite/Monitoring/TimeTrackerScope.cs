using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace GSoft.Dynamite.Monitoring
{
    /// <summary>
    /// Time tracking scope
    /// </summary>
    public class TimeTrackerScope : IDisposable
    {
        private string aggregateKey;
        private IAggregateTimeTracker parentAggregator;
        private Stopwatch stopWatch;

        /// <summary>
        /// Starts a time profiling scope for the specified key.
        /// </summary>
        /// <param name="aggregateKey">The key of the time aggregate we should be adding to while profiling</param>
        /// <param name="parentAggregator">The parent time aggregator</param>
        public TimeTrackerScope(string aggregateKey, IAggregateTimeTracker parentAggregator)
        {
            this.aggregateKey = aggregateKey;
            this.parentAggregator = parentAggregator;
            this.stopWatch = new Stopwatch();
            this.stopWatch.Start();
        }

        /// <summary>
        /// Stops time tracking for the scope
        /// </summary>
        public void Dispose()
        {
            this.stopWatch.Stop();
            this.parentAggregator.AddTimeSpanToAggregateTimeSpentForKey(this.aggregateKey, this.stopWatch.Elapsed);
        }
    }
}
