using System.Collections.Generic;

namespace GSoft.Dynamite.Examples.Server.Rest.Data
{
    public interface ISharePointData<T>
    {
        List<T> GetItems();
        int CacheDurationInSeconds { get; set; }
        int CacheDurationInMinutes { get; set; }
        int CacheDurationInHours { get; set; }
    }
}
