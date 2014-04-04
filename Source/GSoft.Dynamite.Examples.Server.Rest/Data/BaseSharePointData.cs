using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GSoft.Dynamite.Examples.Server.Rest;

namespace GSoft.Dynamite.Examples.Server.Rest.Data
{
    public abstract class BaseSharePointData<T> : ISharePointData<T>
    {
        public abstract List<T> GetItems();              
        private int _cacheDuration = 0;
        public int CacheDurationInSeconds
        {
            get
            {
                return _cacheDuration;
            }
            set
            {
                _cacheDuration = value;
            }
        }
        public int CacheDurationInMinutes
        {
            get
            {
                return CacheDurationInSeconds / 60;
            }
            set
            {
                CacheDurationInSeconds = value * 60;
            }
        }
        public int CacheDurationInHours
        {
            get
            {
                return CacheDurationInMinutes / 60;
            }
            set
            {
                CacheDurationInMinutes = value * 60;
            }
        }
         
    }

}