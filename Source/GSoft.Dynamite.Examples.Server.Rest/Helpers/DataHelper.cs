using System;
using System.Collections.Generic;
using System.Web;
using GSoft.Dynamite.Examples.Server.Rest.Data;

namespace GSoft.Dynamite.Examples.Server.Rest.Helpers
{
    public class DataHelper : IDataHelper
    {
        public List<T> GetItems<T>() where T : ISharePointData<T>, new()
        {
            T objectToCache = new T();
            object cache = HttpContext.Current.Cache[objectToCache.GetType().ToString()];

            if (objectToCache.CacheDurationInSeconds != 0)
            {

                if (cache == null)
                {
                    // Cache is not yet filled, so retrieve it
                    List<T> itemsRetrieved = objectToCache.GetItems();

                    HttpContext.Current.Cache.Insert(
                            objectToCache.GetType().ToString(),
                            itemsRetrieved,
                            null,
                            DateTime.Now.AddSeconds(objectToCache.CacheDurationInSeconds),
                            System.Web.Caching.Cache.NoSlidingExpiration);

                    /* --if using distributed cache such as AppFabric...---
                        * CacheUtility.CurrentCache.Add(
                            objectToCache.GetPropertyBagKeyForCache(),
                                itemsRetrieved,
                                new TimeSpan(0,0,objectToCache.CacheDurationInSeconds));*/

                    return itemsRetrieved;
                }
                else
                {
                    // Return the cached value
                    return (List<T>)cache;
                }
            }
            else
            {
                // Do not get items from the cache
                return objectToCache.GetItems();
            }
        }

        public void ClearItems(string key)
        {
            HttpContext.Current.Cache.Remove(key);
        }
    }
}
