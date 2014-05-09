using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Services.Common;
using System.Runtime.Serialization;
using Microsoft.Office.Server;
using Microsoft.Office.Server.Search.Query;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.Examples.Server.Rest.Data
{
    [Serializable()]
    [DataContract(IsReference = true)]
    [DataServiceKey("UserName")]
    public class UserProfileData : BaseSharePointData<UserProfileData>
    {
        public UserProfileData()
        {
            CacheDurationInMinutes = 10;
        }
        public UserProfileData(string userName, string email, string pictureURL, string path, string jobTitle)
            : base()
        {
            UserName = userName;
            PictureURL = pictureURL;
            Email = email;
            Path = path;
            JobTitle = jobTitle;
        }


        [DataMember()]
        public string UserName { get; set; }
        [DataMember()]
        public string FirstName { get; set; }
        [DataMember()]
        public string LastName { get; set; }
        [DataMember()]
        public string Email { get; set; }
        [DataMember()]
        public string PictureURL { get; set; }
        [DataMember()]
        public string Path { get; set; }
        [DataMember()]
        public string JobTitle { get; set; }

        public override List<UserProfileData> GetItems()
        {
            List<UserProfileData> allProfiles = new List<UserProfileData>();
            using (var query = new FullTextSqlQuery(ServerContext.Current))
            {
                query.QueryText =
                    "SELECT AccountName,FirstName,LastName, WorkEmail,PictureUrl,Path,JobTitle FROM Scope()";
                query.ResultTypes = ResultType.RelevantResults;
                query.RowLimit = 20000;
                ResultTableCollection results = query.Execute();
                ResultTable queryResultsTable = results[ResultType.RelevantResults];
                DataTable queryDataTable = new DataTable();
                queryDataTable.Load(queryResultsTable, LoadOption.OverwriteChanges);

                AddProfiles(allProfiles, queryDataTable.Rows);

            }

            return allProfiles;
        }

        private void AddProfiles(List<UserProfileData> profiles, DataRowCollection rows)
        {
            for (int i = 0; i < rows.Count; i++)
            {

                DataRow row = rows[i];
                profiles.Add(new UserProfileData
                {

                    UserName = (row["AccountName"] != null) ? row["AccountName"].ToString() : string.Empty,
                    FirstName = (row["FirstName"] != null) ? row["FirstName"].ToString() : string.Empty,
                    LastName = (row["LastName"] != null) ? row["LastName"].ToString() : string.Empty,
                    Email = (row["WorkEmail"] != null) ? row["WorkEmail"].ToString() : string.Empty,
                    PictureURL = (row["PictureURL"] != null) ? row["PictureURL"].ToString() : string.Empty,
                    JobTitle = (row["JobTitle"] != null) ? row["JobTitle"].ToString() : string.Empty,
                    Path = (row["Path"] != null) ? row["Path"].ToString() : string.Empty
                });
            }
        }

    }
}
