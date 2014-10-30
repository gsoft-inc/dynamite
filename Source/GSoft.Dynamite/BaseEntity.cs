using System;
using GSoft.Dynamite.Binding;
using GSoft.Dynamite.Fields.Constants;
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
        /// Created date    
        /// </summary>
        [Property(BindingType = BindingType.ReadOnly)]
        public DateTime Modified { get; set; }

        /// <summary>
        /// Gets or sets the Content Type name associated
        /// </summary>
        /// <value>
        /// The Content Type name associated.
        /// </value>
        [Property(BuiltInFields.ContentTypeName, BindingType = BindingType.ReadOnly)]
        public string ContentTypeName { get; set; }
    } 
}
