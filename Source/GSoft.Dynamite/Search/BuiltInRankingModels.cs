using System;

namespace GSoft.Dynamite.Search
{
    /// <summary>
    /// Class to hold the IDs of the built-in ranking models. 
    /// For more info, see : https://technet.microsoft.com/en-ca/library/dn169065.aspx
    /// </summary>
    public static class BuiltInRankingModels
    {
        /// <summary>
        /// Gets the Default Search Model ID. This ranking model is used for most general purposes query.
        /// For example, it is used with the built-in result source "Local SharePoint Results".
        /// </summary>
        public static Guid DefaultSearchModelId
        {
            get { return new Guid("8f6fd0bc-06f9-43cf-bbab-08c377e083f4"); }
        }

        /// <summary>
        /// Gets the pre-August 2013 update default search model. This model is still available for backward compatibility.
        /// </summary>
        public static Guid O15DefaultSearchModelId
        {
            get { return new Guid("9b911c3e-78e1-4b99-9b1f-a69d3691bdd1"); }
        }

        /// <summary>
        /// Gets the SharePoint 2010 default search model. This model is still available for backward compatibility.
        /// </summary>
        public static Guid O14DefaultSearchModelId
        {
            get { return new Guid("9399df62-f089-4033-bdc5-a7ea22936e8e"); }
        }

        /// <summary>
        /// Gets the default ranking model for people search.
        /// </summary>
        public static Guid PeopleSearchDefaultModelId
        {
            get { return new Guid("d9bfb1a1-9036-4627-83b2-bbd9983ac8a1"); }
        }

        /// <summary>
        /// Gets the ranking model for a popularity based search. This model ranks a search result 
        /// based on the number of times the item has been accessed.
        /// </summary>
        public static Guid PopularityRankingModelId
        {
            get { return new Guid("d4ac6500-d1d0-48aa-86d4-8fe9a57a74af"); }
        }
    }
}
