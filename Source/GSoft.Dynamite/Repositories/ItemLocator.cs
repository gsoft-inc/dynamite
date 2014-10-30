using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSoft.Dynamite.Repositories
{
    using GSoft.Dynamite.Fields.Constants;
    using GSoft.Dynamite.Lists;
    using JohnHolliday.Caml.Net;
    using Microsoft.SharePoint;

    /// <summary>
    /// The item locator.
    /// </summary>
    public class ItemLocator : IItemLocator
    {
        private readonly IListLocator listLocator;

        /// <summary>
        /// Utility to find items by name or url
        /// </summary>
        /// <param name="listLocator">List locator utility</param>
        public ItemLocator(IListLocator listLocator)
        {
            this.listLocator = listLocator;
        }

        /// <summary>
        /// Get the list item corresponding to the given title 
        /// </summary>
        /// <param name="web">The web.</param>
        /// <param name="listUrl">The display name to reach the list.</param>
        /// <param name="itemTitle">The title of the list item.</param>
        /// <returns>
        /// The <see cref="SPSecurableObject"/>.
        /// </returns>
        public SPSecurableObject GetByTitle(SPWeb web, string listUrl, string itemTitle)
        {
            SPList list = this.listLocator.GetByUrl(web, listUrl);

            SPQuery query = new SPQuery();

            query.Query =
                CAML.Where(
                    CAML.Or(
                        CAML.Eq(CAML.FieldRef(BuiltInFields.TitleName), CAML.Value(itemTitle)),
                        CAML.Eq(CAML.FieldRef(BuiltInFields.FileLeafRefName), CAML.Value(itemTitle))));

            var itemCollection = list.GetItems(query);

            return itemCollection.Count > 0 ? itemCollection[0] : null;
        }
    }
}