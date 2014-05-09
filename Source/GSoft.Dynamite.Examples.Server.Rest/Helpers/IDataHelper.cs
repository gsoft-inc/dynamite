using System.Collections.Generic;
using GSoft.Dynamite.Examples.Server.Rest.Data;

namespace GSoft.Dynamite.Examples.Server.Rest.Helpers
{
    public interface IDataHelper
    {
        List<T> GetItems<T>() where T : ISharePointData<T>, new();
        void ClearItems(string key);
    }
}