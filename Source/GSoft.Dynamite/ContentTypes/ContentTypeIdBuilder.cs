using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.ContentTypes
{
    /// <summary>
    /// Static utility to help you build properly formatted Content Type identifiers
    /// </summary>
    public static class ContentTypeIdBuilder
    {
        /// <summary>
        /// Creates a new child content type ID
        /// </summary>
        /// <param name="parentContentTypeId">The parent CT ID</param>
        /// <param name="childContentTypeUniqueId">The unique ID for the new child CT</param>
        /// <returns>The new content type ID</returns>
        public static SPContentTypeId CreateChild(SPContentTypeId parentContentTypeId, Guid childContentTypeUniqueId)
        {
            if (parentContentTypeId == null)
            {
                throw new ArgumentNullException("parentContentTypeId");
            }

            if (childContentTypeUniqueId == null || childContentTypeUniqueId == Guid.Empty)
            {
                throw new ArgumentNullException("childContentTypeUniqueId");
            }

            string childGuidString = childContentTypeUniqueId.ToString("N", CultureInfo.InvariantCulture).ToUpperInvariant();
            string parentId = parentContentTypeId.ToString();

            string newContentTypeIdString = string.Format(
                CultureInfo.InvariantCulture,
                "{0}00{1}",     // "00" as separator between CT Guids
                parentId,
                childGuidString);

            return new SPContentTypeId(newContentTypeIdString);
        }

        /// <summary>
        /// Creates a new child content type ID
        /// </summary>
        /// <param name="parentContentTypeId">The parent CT ID</param>
        /// <param name="childContentTypeUniqueId">The unique ID for the new child CT</param>
        /// <returns>The new content type ID</returns>
        public static SPContentTypeId CreateChild(SPContentTypeId parentContentTypeId, int childContentTypeUniqueId)
        {
            if (parentContentTypeId == null)
            {
                throw new ArgumentNullException("parentContentTypeId");
            }

            if (childContentTypeUniqueId <= 0)
            {
                throw new ArgumentOutOfRangeException("childContentTypeUniqueId", "Child content type discriminator integer should be larger than 0.");
            }

            string childIdString = childContentTypeUniqueId.ToString(CultureInfo.InvariantCulture);
            string parentId = parentContentTypeId.ToString();

            string newContentTypeIdString = string.Format(
                CultureInfo.InvariantCulture,
                "{0}0{1}",      // "0" as separator between CT int IDs
                parentId,
                childIdString);

            return new SPContentTypeId(newContentTypeIdString);
        }

        /// <summary>
        /// Creates a new child content type ID
        /// </summary>
        /// <param name="parentContentTypeId">The parent CT ID</param>
        /// <param name="childContentTypeUniqueId">The unique ID for the new child CT</param>
        /// <returns>The new content type ID</returns>
        public static SPContentTypeId CreateChild(SPContentTypeId parentContentTypeId, string childContentTypeUniqueId)
        {
            if (parentContentTypeId == null)
            {
                throw new ArgumentNullException("parentContentTypeId");
            }

            if (!string.IsNullOrEmpty(childContentTypeUniqueId))
            {
                throw new ArgumentNullException("childContentTypeUniqueId");
            }

            string childIdString = childContentTypeUniqueId.ToString(CultureInfo.InvariantCulture);
            string parentId = parentContentTypeId.ToString();

            string newContentTypeIdString = string.Format(
                CultureInfo.InvariantCulture,
                "{0}{1}",       // use no separator when you use a dirty string-based CT discriminator: at this point, your loss your CTID logic is messed up. 
                parentId,
                childIdString);

            return new SPContentTypeId(newContentTypeIdString);
        }
    }
}
