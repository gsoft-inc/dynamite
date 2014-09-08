using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GSoft.Dynamite.Logging;
using GSoft.Dynamite.Utils;

namespace GSoft.Dynamite.Monitoring
{
    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class AggregateTimeTracker : IAggregateTimeTracker
    {
        private static readonly NamedReaderWriterLocker<string> NamedLocker = new NamedReaderWriterLocker<string>();

        private IDictionary<string, TimeSpan> aggregateTimeSpans = new Dictionary<string, TimeSpan>();
        private ILogger log;

        public AggregateTimeTracker(ILogger log)
        {
            this.log = log;
        }
        
        public TimeSpan GetAggregateTimeSpentForKey(string key)
        {
            return this.EnsureTimeSpanForKey(key);
        }

        public void AddTimeSpanToAggregateTimeSpentForKey(string key, TimeSpan timeSpan)
        {
            // Don't allow two threads to update aggregate at once
            NamedLocker.RunWithWriteLock(
                key,
                () =>
                {
                    // Do the same thing as EnsureTimeSpanForKey, except we already have the writer lock
                    if (!this.aggregateTimeSpans.ContainsKey(key))
                    {
                        this.aggregateTimeSpans[key] = new TimeSpan();
                    }

                    var aggregate = this.aggregateTimeSpans[key].Add(timeSpan);
                    this.aggregateTimeSpans[key] = aggregate;

                    this.log.Info("Aggregate Time Tracker - Key: {0}, Total millis:{1}", key, aggregate.TotalMilliseconds);
                });
        }

        public TimeTrackerScope BeginTimeTrackerScope(string key)
        {
            return new TimeTrackerScope(key, this);
        }

        private TimeSpan EnsureTimeSpanForKey(string key)
        {
            return NamedLocker.RunWithUpgradeableReadLock(
                   key,
                   () =>
                   {
                       if (!this.aggregateTimeSpans.ContainsKey(key))
                       {
                           return NamedLocker.RunWithWriteLock(
                               key,
                               () =>
                               {

                                   // Double check for thread concurency, somone might have aquired write lock before we did
                                   if (!this.aggregateTimeSpans.ContainsKey(key))
                                   {
                                       this.aggregateTimeSpans[key] = new TimeSpan();
                                   }

                                   return this.aggregateTimeSpans[key];
                               });
                       }

                       // Return the existing TimeSpan
                       return this.aggregateTimeSpans[key];
                   });
        }
    }
}
