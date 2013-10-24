using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel.Web;
using System.Web;
using GSoft.Dynamite.Examples.Server.Rest.Data;
using GSoft.Dynamite.Examples.Server.Rest.Helpers;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.Examples.Server.Rest.Services
{
    public class CacheDataContext
    {
        IDataHelper dataHelper = new DataHelper();

        public IQueryable<UserProfileData> UserProfiles
        {
            get
            {
                return dataHelper.GetItems<UserProfileData>().AsQueryable();
            }
        }


        //public IQueryable<ListResult> ListResults
        //{
        //    get
        //    {
        //        return dataHelper.GetItems<ListResult>().AsQueryable();
        //    }
        //}
    }
}
