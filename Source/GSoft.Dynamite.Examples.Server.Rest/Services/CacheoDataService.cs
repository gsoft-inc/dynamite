using System;
using System.Collections.Generic;
using System.Data.Services;
using System.Data.Services.Common;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using GSoft.Dynamite.Examples.Server.Rest.Data;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Client.Services;

namespace GSoft.Dynamite.Examples.Server.Rest.Services
{

    [BasicHttpBindingServiceMetadataExchangeEndpoint]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
    [System.ServiceModel.ServiceBehavior(
    IncludeExceptionDetailInFaults = true)]
    public class CacheoDataService : DataService<CacheDataContext>
    {
        public static void InitializeService(DataServiceConfiguration config)
        {
            try
            {
                config.SetEntitySetAccessRule("*", EntitySetRights.AllRead);

                config.SetServiceOperationAccessRule("GetItemsByTitle", ServiceOperationRights.All);

                config.DataServiceBehavior.MaxProtocolVersion = DataServiceProtocolVersion.V2;
            }
            catch (Exception err)
            {
                string t;
            }
            
        }


        [WebGet]
        public IQueryable<ListResult> GetItemsByTitle(string listName)
        {
            var context = SPContext.Current;

            var listResults = new List<ListResult>();
            SPSecurity.RunWithElevatedPrivileges(delegate()
            {

                using (SPSite site = new SPSite(context.Web.Url))
                {
                    SPWeb web = site.OpenWeb();
                    web.AllowUnsafeUpdates = true;
                    SPList listByTitle = web.Lists[listName];

                    foreach (SPListItem item in listByTitle.Items)
                    {
                        listResults.Add(new ListResult
                        {
                        //    Metadata = new ListResponseMetaData()
                        //    {
                        //        Id = item.ID.ToString(),
                        //        Type = item.GetType().FullName
                        //    },
                            Title = item.Title,
                            Created = DateTime.Parse(item["Created"].ToString()),
                            Description = item["Description"] != null ? item["Description"].ToString() : string.Empty
                        });
                    }
                    web.AllowUnsafeUpdates = false;
                }
            });
            //var response = new ListResponseWrapper()
            //{
            //    ListResponse = new ListResponse()
            //    {
            //        Results = listResults
            //    }
            //};
            return listResults.AsQueryable();
        }


    }

    [DataContract]
    public class ListResponseWrapper
    {
        [DataMember]
        public ListResponse ListResponse;
    }

    [DataContract]
    public class ListResponse
    {
        [DataMember]
        public List<ListResult> Results;
    }

    [DataContract]
    public class ListResponseMetaData
    {
        [DataMember]
        public string Id { get; set; }
        [DataMember]
        public string Type { get; set; }

        [DataMember]
        public Uri Uri { get; set; }
    }

    [Serializable]
    [DataContract(IsReference = true)]
    public class ListResult : BaseSharePointData<ListResult>
    {
        //[DataMember(Name = "__metadata", Order = 1)]
        //public ListResponseMetaData Metadata;

        [DataMember(Order = 2)]
        public string Title;

        [DataMember(Order = 3)]
        public DateTime Created;

        [DataMember(Order = 4)]
        public string Description;

       
        public override List<ListResult> GetItems()
        {
            var result1 = new ListResult()
            {
                Title = "tata",
                Created = DateTime.Now,
                Description = "descasdas"
            };

            var result2 = new ListResult()
            {
                Title = "tata",
                Created = DateTime.Now,
                Description = "descasdas"
            };

            var result3 = new ListResult()
            {
                Title = "tata",
                Created = DateTime.Now,
                Description = "descasdas"
            };
            var result4 = new ListResult()
            {
                Title = "tata",
                Created = DateTime.Now,
                Description = "descasdas"
            }; 
            
            var result5 = new ListResult()
            {
                Title = "tata",
                Created = DateTime.Now,
                Description = "descasdas"
            };

            var list = new List<ListResult>();
            list.Add(result1);
            list.Add(result2);
            list.Add(result3);
            list.Add(result4);
            list.Add(result5);

            return list;

        }
    }
}
