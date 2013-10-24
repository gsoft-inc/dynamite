using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.ServiceModel.Activation;
using System.Web;
using GSoft.Dynamite.Examples.Server.Rest.Services;
using Microsoft.Office.Server.ActivityFeed;
using Microsoft.Office.Server.Search.Administration;
using Microsoft.Office.Server.Search.Query;
using Microsoft.Office.Server.UserProfiles;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Workflow;

namespace GSoft.Dynamite.Examples.Server.Rest
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class Service : IService
    {
        private const string SharepointSiteUrl = "http://spdev-luji/sites/client/";

        public string Test()
        {
            var tata = HttpContext.Current.Request.ApplicationPath;
            var toto = SPContext.Current;

            return "Hello World";
        }

        //public CacheDataContext.ListResponseWrapper GetItemsByTitle(string listName)
        //{
        //    var tata = HttpContext.Current.Request.ApplicationPath;
        //    var toto = SPContext.Current;


        //    var listResults = new List<CacheDataContext.ListResult>();
        //    SPSecurity.RunWithElevatedPrivileges(delegate()
        //    {

        //        using (SPSite site = new SPSite(SharepointSiteUrl))
        //        {
        //            SPWeb web = site.OpenWeb();
        //            web.AllowUnsafeUpdates = true;
        //            SPList listByTitle = web.Lists[listName];

        //            foreach (SPListItem item in listByTitle.Items)
        //            {
        //                listResults.Add(new ListResult
        //                {
        //                    Metadata = new ListResponseMetaData()
        //                    {
        //                        Id = item.ID.ToString(),
        //                        Type = item.GetType().FullName
        //                    },
        //                    Title = item.Title,
        //                    Created = DateTime.Parse(item["Created"].ToString()),
        //                    Description = item["Description"] != null ? item["Description"].ToString() : string.Empty
        //                });
        //            }
        //            web.AllowUnsafeUpdates = false;
        //        }
        //    });
        //    var response = new ListResponseWrapper()
        //    {
        //        ListResponse = new ListResponse()
        //        {
        //            Results = listResults
        //        }
        //    };
        //    return response;
        //}

        //public ListResponseWrapper GetColumnsByTitle(string listName)
        //{
           
        //    var listResults = new List<ListResult>();
        //    SPSecurity.RunWithElevatedPrivileges(delegate()
        //    {

        //        using (SPSite site = new SPSite(SharepointSiteUrl))
        //        {
        //            SPWeb web = site.OpenWeb();
        //            web.AllowUnsafeUpdates = true;
        //            SPList listByTitle = web.Lists.TryGetList(listName);

        //            if (listByTitle != null)
        //            {
        //                foreach (SPField field in listByTitle.Fields)
        //                {
        //                    listResults.Add(new ListResult
        //                    {
        //                        Metadata = new ListResponseMetaData()
        //                        {
        //                            Id = field.Id.ToString(),
        //                            Type = field.GetType().FullName
        //                        },
        //                        Title = field.Title,
        //                        Description = field.Description
        //                    });
        //                }
        //            }
        //            web.AllowUnsafeUpdates = false;
        //        }
        //    });
        //    var response = new ListResponseWrapper()
        //    {
        //        ListResponse = new ListResponse()
        //        {
        //            Results = listResults
        //        }
        //    };
        //    return response;
        //}

        public string AddListItem(string listName, string title, string description)
        {
            SPSecurity.RunWithElevatedPrivileges(() =>
            {
                using (var site = new SPSite(SharepointSiteUrl))
                {
                    using (var web = site.OpenWeb())
                    {
                        SPList ticketList = web.Lists.TryGetList(listName);
                        web.AllowUnsafeUpdates = true;
                        if (ticketList != null)
                        {
                            SPListItem ticket = ticketList.Items.Add();
                            ticket["Title"] = title;
                            ticket["Description"] = description;
                            ticket.Update();
                        }
                        web.AllowUnsafeUpdates = false;
                    }
                }
            });
            return "Success Message";

        }

        public string AssignPermissions(string userName, string siteName, string permissionLevelName)
        {
            SPSecurity.RunWithElevatedPrivileges(() =>
            {
                using (var siteCollection = new SPSite(SharepointSiteUrl))
                {
                    using (var web = siteCollection.OpenWeb(siteName))
                    {
                        web.AllowUnsafeUpdates = true; 
                        var spUsers = web.SiteUsers; 
                        var sPUser = spUsers[userName]; 
                        var spRoleAss = new SPRoleAssignment(sPUser); 
                        web.BreakRoleInheritance(true); 
                        spRoleAss.RoleDefinitionBindings.Add(web.RoleDefinitions[permissionLevelName]); 
                        web.RoleAssignments.Add(spRoleAss); 
                        web.AllowUnsafeUpdates = false;
                    }
                }
            }); return "Success Message"; }

        public SearchResponseWrapper Search(string searchText) { 
            var searchRows = new List<SearchTableRowsResult>(); 
            
            using (var site = new SPSite(SharepointSiteUrl)) { 
                var web = site.OpenWeb(); 
                web.AllowUnsafeUpdates = true;
                var ssaProxy = (SearchServiceApplicationProxy)SearchServiceApplicationProxy. GetProxy(SPServiceContext.GetContext(new SPSite(SharepointSiteUrl))); 
                var keywordQuery = new KeywordQuery(ssaProxy); 
                keywordQuery.ResultsProvider = SearchProvider.SharepointSearch; 
                keywordQuery.QueryText = searchText + " +isDocument:1"; 
                keywordQuery.HiddenConstraints = "site:\"" + SharepointSiteUrl + "\""; 
                keywordQuery.KeywordInclusion = KeywordInclusion.AllKeywords; 
                keywordQuery.ResultTypes |= ResultType.RelevantResults; 
                keywordQuery.SelectProperties.Add("Title"); 
                keywordQuery.SelectProperties.Add("Path"); 
                keywordQuery.SelectProperties.Add("HitHighlightedSummary"); 
                var searchResults = keywordQuery.Execute(); 
                if (searchResults.Exists(ResultType.RelevantResults)) { 
                    var searchResultTable = searchResults[ResultType.RelevantResults]; 
                    var result = new DataTable {TableName = "SearchResults"};
                    result.Load(searchResultTable, LoadOption.OverwriteChanges);
                    searchRows.AddRange(from DataRow resultRow in result.Rows
                        select new SearchTableRowsResult()
                        {
                            Value = resultRow.ItemArray[0].ToString(),
                        });
                } 
                
                web.AllowUnsafeUpdates = false; 
            } 
            
            var response = new SearchResponseWrapper()
            {
                SearchResponse = new SearchResponse()
                {
                    Query = new SearchResult()
                    {
                        Metadata = new SearchResponseMetaData() { }, 
                        PrimaryQueryResult = new SearchPrimaryQueryResult()
                        {
                            RelevantResults = new SearchRelevantResults() { Table = new SearchTable()
                            {
                                Rows = new SearchTableRows()
                                {
                                    Results = searchRows
                                }
                            } }
                        }
                    }
                }
            }; 
            return response; 
        }

        public SocialFeedResponseWrapper GetMyActivities(string currentUserName)
        {
            var socialThreads = new List<SocialThread>();
            SPSecurity.RunWithElevatedPrivileges(() =>
            {
                using (var sitecollection = new SPSite(SharepointSiteUrl))
                {
                    var currentcontext = SPServiceContext.GetContext(sitecollection); 
                    var userprofmanager = new UserProfileManager(currentcontext); 
                    var currentuser = userprofmanager.GetUserProfile(currentUserName); 
                    var activitymanager = new ActivityManager(currentuser, currentcontext); 
                    var eventscollection = activitymanager.GetActivitiesByMe();
                    foreach (ActivityEvent activity in eventscollection)
                    {
                        if (activity.LinksList != null)
                        {
                            socialThreads.Add(new SocialThread()
                            {
                                Actors = new SocialThreadActor()
                                {
                                    Results = new List<SocialThreadActorResult>()
                                    {
                                        new SocialThreadActorResult()
                                        {
                                            Name = activity.Publisher.Name
                                        }
                                    }
                                }, RootPost = new SocialThreadRootPost()
                                {
                                    Text = activity.Value,
                                }
                            });
                        }
                    }
                }
            }); 

            var response = new SocialFeedResponseWrapper()
            {
                SocialResponse = new SocialResponse()
                {
                    Metadata = new SocialResponseMetaData() { }, 
                    SocialFeed = new SocialResult()
                    {
                        Threads = socialThreads
                    }
                }
            }; 
            
            return response;
        }

        public string AssociateWorkflow(string associationListName, string taskListName, string historyListName, string workflowName)
        {
            SPSecurity.RunWithElevatedPrivileges(() =>
            {
                using (var spSite = new SPSite(SharepointSiteUrl))
                {
                    using (var spWeb = spSite.OpenWeb())
                    {
                        var workflowTemplate = spWeb.WorkflowTemplates.GetTemplateByName(workflowName, System.Globalization.CultureInfo.CurrentCulture); 
                        var taskList = spWeb.Lists[taskListName]; 
                        var historyList = spWeb.Lists[historyListName]; 
                        var workflowAssociation = SPWorkflowAssociation.CreateListAssociation(workflowTemplate, workflowName, taskList, historyList); 
                        workflowAssociation.AutoStartChange = true; 
                        workflowAssociation.AutoStartCreate = true; 
                        workflowAssociation.AllowManual = true; 
                        spWeb.AllowUnsafeUpdates = true; 
                        var associatedList = spWeb.Lists[associationListName]; 
                        associatedList.WorkflowAssociations.Add(workflowAssociation); 
                        associatedList.Update(); 
                        spWeb.AllowUnsafeUpdates = false;
                    }
                }
            }); 
            
            return "Success Message";
        }
    }
}