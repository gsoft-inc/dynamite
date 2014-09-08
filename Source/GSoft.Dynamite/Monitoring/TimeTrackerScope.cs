using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace GSoft.Dynamite.Monitoring
{
    public class TimeTrackerScope : IDisposable
    {
        private string aggregateKey;
        private IAggregateTimeTracker parentAggregator;
        private Stopwatch stopWatch;

        public TimeTrackerScope(string aggregateKey, IAggregateTimeTracker parentAggregator)
        {
            this.aggregateKey = aggregateKey;
            this.parentAggregator = parentAggregator;
            this.stopWatch = new Stopwatch();
            this.stopWatch.Start();
        }

        public void Dispose()
        {
            this.stopWatch.Stop();
            this.parentAggregator.AddTimeSpanToAggregateTimeSpentForKey(this.aggregateKey, this.stopWatch.Elapsed);
        }
    }
}
