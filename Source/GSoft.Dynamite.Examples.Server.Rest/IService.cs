using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace GSoft.Dynamite.Examples.Server.Rest
{ 
    [ServiceContract(Name = "Service")]
    public interface IService
    {
        
        [OperationContract]
        [WebInvoke(Method = "GET", BodyStyle = WebMessageBodyStyle.Wrapped,
            ResponseFormat = WebMessageFormat.Json, UriTemplate = "/Test")]
        string Test();

        [OperationContract]
        [WebGet(UriTemplate = "/_api/web/lists/getbytitle('{listName}')/items",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json)]
        ListResponseWrapper GetByTitle(string listName);

        [OperationContract]
        [WebInvoke(Method = "POST", UriTemplate = "/_api/web/lists/getbytitle('{ListName}')/additem",
            ResponseFormat = WebMessageFormat.Json,
            RequestFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        string AddListItem(string listName, string title, string description);

        [OperationContract]
        [WebInvoke(Method = "POST", UriTemplate = "/_api/web/RoleAssignments/add",
            ResponseFormat = WebMessageFormat.Json,
            RequestFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.WrappedRequest
        )]
        string AssignPermissions(string userName, string siteName, string permissionLevelName);

        [OperationContract]
        [WebGet(UriTemplate = "/_api/search/query/querytext/'{searchText}'",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json)]
        SearchResponseWrapper Search(string searchText);

        [OperationContract]
        [WebInvoke(Method = "POST", UriTemplate = "/_api/socialfeed/my/feed",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        SocialFeedResponseWrapper GetMyActivities(string currentUserName);

        //[OperationContract]
        //[WebInvoke(Method = "POST", UriTemplate = "/_api/web/lists/getbytitle('{ListName}')/workflowassociations/add",
        //    RequestFormat = WebMessageFormat.Json,
        //    ResponseFormat = WebMessageFormat.Json,
        //    BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        //string AssociateWorkflow(string associationListName, string taskListName, string historyListName, string workflowName);
    }

    [DataContract]
    public class SocialFeedResponseWrapper
    {
        [DataMember] public SocialResponse SocialResponse;
    }

    [DataContract]
    public class SocialResponse
    {
        [DataMember(Name = "__metadata", Order = 1)] 
        public SocialResponseMetaData Metadata; 
        
        [DataMember] 
        public SocialResult SocialFeed;
    }

    [DataContract]
    public class SocialResponseMetaData
    {
        [DataMember]
        public int Id { get; set; } 
        
        [DataMember] 
        public string Type { get; set; } 
        
        [DataMember]
        public Uri Uri { get; set; }
    }

    [DataContract]
    public class SocialResult
    {
        [DataMember] 
        public List<SocialThread> Threads;
    }

    [DataContract]
    public class SocialThread
    {
        [DataMember] 
        public SocialThreadActor Actors { get; set; }
        
        [DataMember]
        public SocialThreadRootPost RootPost { get; set; }
    }

    [DataContract]
    public class SocialThreadActor
    {
        [DataMember] 
        public List<SocialThreadActorResult> Results { get; set; }
    }

    [DataContract]
    public class SocialThreadActorResult
    {
        [DataMember]
        public string Name { get; set; }
    }

    [DataContract]
    public class SocialThreadRootPost
    {
        [DataMember] 
        public string Text { get; set; }
    }

    [DataContract]
    public class SearchResponseWrapper
    {
        [DataMember] 
        public SearchResponse SearchResponse;
    }

    [DataContract]
    public class SearchResponse
    {
        [DataMember] 
        public SearchResult Query;
    }

    [DataContract]
    public class SearchResponseMetaData
    {
        [DataMember] 
        public string Type { get; set; }
    }

    [DataContract]
    public class SearchPrimaryQueryResult
    {
        [DataMember] 
        public SearchRelevantResults RelevantResults { get; set; }
    }

    [DataContract]
    public class SearchRelevantResults
    {
        [DataMember] 
        public SearchTable Table { get; set; }
    }

    [DataContract]
    public class SearchTable
    {
        [DataMember] 
        public SearchTableRows Rows { get; set; }
    }

    [DataContract]
    public class SearchTableRows
    {
        [DataMember] 
        public List<SearchTableRowsResult> Results { get; set; }
    }

    [DataContract]
    public class SearchTableRowsResult
    {
        [DataMember] 
        public string Value { get; set; }
    }

    [DataContract]
    public class SearchResult
    {
        [DataMember(Name = "__metadata", Order = 1)] 
        public SearchResponseMetaData Metadata; 

        [DataMember(Order = 2)] 
        public SearchPrimaryQueryResult PrimaryQueryResult;
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
        public int Id { get; set; }
        [DataMember]
        public string Type { get; set; }

        [DataMember]
        public Uri Uri { get; set; }
    }

    [DataContract]
    public class ListResult
    {
        [DataMember(Name = "__metadata", Order = 1)]
        public ListResponseMetaData Metadata;

        [DataMember(Order = 2)]
        public string Title;

        [DataMember(Order = 3)]
        public DateTime Created;

        [DataMember(Order = 4)]
        public string Description;
    }
}