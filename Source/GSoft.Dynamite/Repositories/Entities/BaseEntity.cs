using System;
using GSoft.Dynamite.Binding;

namespace GSoft.Dynamite.Repositories.Entities
{
    /// <summary>
    /// Base entity, parent class for all content type-fed entities
    /// </summary>
    public class BaseEntity
    {
        /// <summary>
        /// List item ID
        /// </summary>
        [Property("ID", BindingType = BindingType.ReadOnly)]
        public int Id { get; set; }

        /// <summary>
        /// List item title
        /// </summary>
        [Property]
        public string Title { get; set; }

        /// <summary>
        /// Date and time of item creation
        /// </summary>
        [Property(BindingType = BindingType.ReadOnly)]
        public DateTime Created { get; set; }
    }
}
