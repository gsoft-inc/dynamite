using System;
using GSoft.Dynamite.Binding;

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
    } 
}
