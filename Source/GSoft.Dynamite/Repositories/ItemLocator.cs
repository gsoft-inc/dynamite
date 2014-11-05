using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GSoft.Dynamite.Caml;
using GSoft.Dynamite.Fields.Constants;
using GSoft.Dynamite.Lists;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.Repositories
{
    /// <summary>
    /// The item locator.
    /// </summary>
    public class ItemLocator : IItemLocator
    {
        private readonly IListLocator listLocator;
        private readonly ICamlBuilder caml;

        /// <summary>
        /// Utility to find items by name or url
        /// </summary>
        /// <param name="listLocator">List locator utility</param>
        /// <param name="caml">CAML building utility</param>
        public ItemLocator(IListLocator listLocator, ICamlBuilder caml)
        {
            this.listLocator = listLocator;
            this.caml = caml;
        }

        /// <summary>
        /// Get the list item corresponding to the given title 
        /// </summary>
        /// <param name="web">The web.</param>
        /// <param name="listUrl">The web-relative URL of the .</param>
        /// <param name="itemTitle">The title of the list item.</param>
        /// <returns>
        /// The <see cref="SPSecurableObject"/>.
        /// </returns>
        public SPSecurableObject GetByTitle(SPWeb web, string listUrl, string itemTitle)
        {
            return this.GetByTitle(web, new Uri(listUrl, UriKind.Relative), itemTitle);
        }
        
        /// <summary>
        /// Get the list item corresponding to the given title 
        /// </summary>
        /// <param name="web">The web.</param>
        /// <param name="listUrl">The list path to reach the list.</param>
        /// <param name="itemTitle">The title of the list item.</param>
        /// <returns>
        /// The <see cref="SPSecurableObject"/>.
        /// </returns>
        public SPSecurableObject GetByTitle(SPWeb web, Uri listUrl, string itemTitle)
        {
            SPList list = this.listLocator.GetByUrl(web, listUrl);

            SPQuery query = new SPQuery();

            query.Query =
                this.caml.Where(
                    this.caml.Or(
                        this.caml.Equal(this.caml.FieldRef(BuiltInFields.TitleName), this.caml.Value(itemTitle)),
                        this.caml.Equal(this.caml.FieldRef(BuiltInFields.FileLeafRefName), this.caml.Value(itemTitle))));

            var itemCollection = list.GetItems(query);

            return itemCollection.Count > 0 ? itemCollection[0] : null;
        }
    }
}