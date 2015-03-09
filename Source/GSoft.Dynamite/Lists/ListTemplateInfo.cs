using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSoft.Dynamite.Lists
{
    /// <summary>
    /// Class to store a list template ID and its related feature ID
    /// </summary>
    public class ListTemplateInfo
    {
        /// <summary>
        /// Default constructor for serialization purposes
        /// </summary>
        public ListTemplateInfo()
        {
        }

        /// <summary>
        /// List Template constructor
        /// </summary>
        /// <param name="listTemplateTypeId">List template Type (raw integer value)</param>
        /// <param name="featureId">List template type related feature ID</param>
       public ListTemplateInfo(int listTemplateTypeId, Guid featureId)
        {
            this.ListTempateTypeId = listTemplateTypeId;
            this.FeatureId = featureId;
        }

        /// <summary>
       /// List template Type (raw integer value)
        /// </summary>
        public int ListTempateTypeId { get; set; }

        /// <summary>
        /// List template type related feature ID
        /// </summary>
        public Guid FeatureId { get; set; }
    }
}
