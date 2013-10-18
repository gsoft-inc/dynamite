using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls.WebParts;

namespace GSoft.Dynamite.Examples.Parts.BasicWall
{
    /// <summary>
    /// Basic wall web part
    /// </summary>
    [ToolboxItem(false)]
    public class BasicWallWebPart : WebPart
    {
        // Visual Studio might automatically update this path when you change the Visual Web Part project item.
        private const string AscxPath = @"~/_CONTROLTEMPLATES/GSoft.Dynamite.Examples.Parts/BasicWall/BasicWallUserControl.ascx";

        /// <summary>
        /// Creates child controls
        /// </summary>
        protected override void CreateChildControls()
        {
            Control control = Page.LoadControl(AscxPath);
            Controls.Add(control);
        }
    }
}
