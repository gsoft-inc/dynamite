using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.ContentTypes.Constants
{
    /// <summary>
    /// Content types ID's constants for built-in (OOTB) content types
    /// </summary>
    public static class BuiltInContentTypes
    {
        /// <summary>
        /// SharePoint Base Item content type
        /// </summary>
        public static SPContentTypeId Item
        {
            get { return SPBuiltInContentTypeId.Item; }
        }

        /// <summary>
        /// SharePoint Base Document  content type
        /// </summary>
        public static SPContentTypeId Document
        {
            get { return SPBuiltInContentTypeId.Document; }
        }

        /// <summary>
        /// SharePoint Base Image document content type
        /// </summary>
        public static SPContentTypeId Image
        {
            get { return new SPContentTypeId(BuiltInContentTypes.Item.ToString() + "0102"); }
        }

        /// <summary>
        /// SharePoint Base Video document content type
        /// </summary>
        public static SPContentTypeId Video
        {
            get { return new SPContentTypeId(BuiltInContentTypes.Item.ToString() + "20D520A808"); }
        }

        /// <summary>
        /// SharePoint Base Rich Media asset content type
        /// </summary>
        public static SPContentTypeId RichMediaAsset
        {
            get { return new SPContentTypeId(BuiltInContentTypes.Document.ToString() + "009148F5A04DDD49CBA7127AADA5FB792B"); }
        }

        /// <summary>
        /// SharePoint Base Rich Image document content type
        /// </summary>
        public static SPContentTypeId RichImage
        {
            get { return new SPContentTypeId(BuiltInContentTypes.RichMediaAsset.ToString() + "00AADE34325A8B49CDA8BB4DB53328F214"); }
        }

        /// <summary>
        /// SharePoint Base Rich Audio document content type
        /// </summary>
        public static SPContentTypeId RichAudio
        {
            get { return new SPContentTypeId(BuiltInContentTypes.RichMediaAsset.ToString() + "006973ACD696DC4858A76371B2FB2F439A"); }
        }

        /// <summary>
        /// SharePoint Base Video Rendition document content type
        /// </summary>
        public static SPContentTypeId RichVideoRendition
        {
            get { return new SPContentTypeId(BuiltInContentTypes.RichMediaAsset.ToString() + "00291D173ECE694D56B19D111489C4369D"); }
        }
    }
}
