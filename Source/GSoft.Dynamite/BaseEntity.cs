using System;
using GSoft.Dynamite.Binding;
using Microsoft.SharePoint;

namespace GSoft.Dynamite
{
    /// <summary>
    /// Base class for SPListItem-mapped entities
    /// </summary>
    public class BaseEntity
    {
        /// <summary>
        /// Item identifier within its list
        /// </summary>
        [Property("ID", BindingType = BindingType.ReadOnly)]
        public int Id { get; set; }

        /// <summary>
        /// Item title
        /// </summary>
        [Property]
        public string Title { get; set; }

        /// <summary>
        /// Created date    
        /// </summary>
        [Property(BindingType = BindingType.ReadOnly)]
        public DateTime Created { get; set; }

        /// <summary>
        /// Gets or sets the Content Type Id associated
        /// </summary>
        /// <value>
        /// The Content Type Id associated.
        /// </value>
        [Property(BuiltInFields.ContentTypeIdName, BindingType = BindingType.ReadOnly)]
        public SPContentTypeId ContentTypeId { get; set; }
    } 
}
