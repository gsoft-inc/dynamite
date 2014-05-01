using Microsoft.SharePoint;

namespace GSoft.Dynamite.Binding
{
    using System.Data;

    /// <summary>
    /// Values that are loaded from a SharePoint list.
    /// </summary>
    public interface IDataRowValues
    {
        /// <summary>
        /// Gets the data row.
        /// </summary>
        DataRow DataRow { get; }
    }
}
