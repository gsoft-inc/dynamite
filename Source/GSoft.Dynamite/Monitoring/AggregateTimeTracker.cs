using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GSoft.Dynamite.Logging;
using GSoft.Dynamite.Utils;

namespace GSoft.Dynamite.Monitoring
{
    /// <summary>
    /// Aggregate time tracking utility
    /// </summary>
    public class AggregateTimeTracker : IAggregateTimeTracker
    {
        private static readonly NamedReaderWriterLocker<string> NamedLocker = new NamedReaderWriterLocker<string>();

        private IDictionary<string, TimeSpan> aggregateTimeSpans = new Dictionary<string, TimeSpan>();
        private ILogger log;

        /// <summary>
        /// Summing Time Tracker
        /// </summary>
        /// <param name="log">The logging utility</param>
        public AggregateTimeTracker(ILogger log)
        {
            this.log = log;
        }
        
        /// <summary>
        /// Fetches the total time spent in code segments that
        /// were timed using the specified key
        /// </summary>
        /// <param name="key">The key that was used to track time</param>
        /// <returns>The sum time span</returns>
        public TimeSpan GetAggregateTimeSpentForKey(string key)
        {
            return this.EnsureTimeSpanForKey(key);
        }

        /// <summary>
        /// Adds time to the total for the specified key
        /// </summary>
        /// <param name="key">The key of the timings aggregate</param>
        /// <param name="timeSpan">The time spent that must be added</param>
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

        /// <summary>
        /// Starts a time tracking scope
        /// </summary>
        /// <param name="key">The aggregate timings key</param>
        /// <returns>The disposable timing scope</returns>
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
