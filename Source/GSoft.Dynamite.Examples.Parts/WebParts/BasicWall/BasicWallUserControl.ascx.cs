using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using GSoft.Dynamite.Examples.Entities;
using GSoft.Dynamite.Examples.Unity;
using GSoft.Dynamite.Examples.ViewModels;
using GSoft.Dynamite.ValueTypes;
using Microsoft.Practices.Unity;

namespace GSoft.Dynamite.Examples.Parts.BasicWall
{
    /// <summary>
    /// Basic wall user control
    /// </summary>
    public partial class BasicWallUserControl : UserControl
    {
        private List<TextBox> postbackTextBoxes;

        /// <summary>
        /// Constructor to initialize dependency
        /// </summary>
        public BasicWallUserControl()
        {
            this.ViewModel = AppContainer.Current.Resolve<WallViewModel>();
        }

        /// <summary>
        /// Control's view model
        /// </summary>
        public WallViewModel ViewModel { get; set; }

        /// <summary>
        /// Fires at page load
        /// </summary>
        /// <param name="sender">Origin of event</param>
        /// <param name="e">Event arguments</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            this.NewPost.Attributes["placeholder"] = this.ViewModel.PostPlaceholderText;
            this.NewPostSubmit.Text = this.ViewModel.PostSubmitText;

            // We assume ViewState is off in web.config. This is .NET 3.5 so we can't enable ViewState granuraly
            // on specific controls.
            // So, we need re-bind our repeater everytime otherwise the event handlers won't fire. However, the 
            // re-bind clears our control's values, so we need to store the text box values before re-binding.
            if (Page.IsPostBack)
            {
                this.postbackTextBoxes = new List<TextBox>();

                foreach (RepeaterItem item in this.PostRepeater.Items)
                {
                    if ((item.ItemType == ListItemType.Item) || (item.ItemType == ListItemType.AlternatingItem))
                    {
                        TextBox txt = item.FindControl("NewReply") as TextBox;
                        this.postbackTextBoxes.Add(txt);
                    }
                }
            }

            this.FetchAndBind();
        }

        /// <summary>
        /// Fires when the NewPost button is clicked
        /// </summary>
        /// <param name="sender">Origin of event</param>
        /// <param name="e">Event arguments</param>
        protected void NewPostButton_Click(object sender, EventArgs e)
        {
            this.ViewModel.AddPost(this.NewPost.Text);
            this.NewPost.Text = string.Empty;

            // We updated the contents, so we need to re-bind otherwise the refreshed page won't show our changes
            this.FetchAndBind();
        }

        /// <summary>
        /// Fires when a wall post is databound
        /// </summary>
        /// <param name="sender">Origin of event</param>
        /// <param name="e">Event arguments</param>
        protected void PostRepeater_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            // Bind the nested repeater
            RepeaterItem item = e.Item;
            if ((item.ItemType == ListItemType.Item) ||
                (item.ItemType == ListItemType.AlternatingItem))
            {
                Repeater innerRepeater = (Repeater)item.FindControl("ReplyRepeater");
                WallPost currentPost = (WallPost)item.DataItem;
                innerRepeater.DataSource = currentPost.Replies;
                innerRepeater.DataBind();

                Literal replyOpenForm = (Literal)item.FindControl("NewReplyOpenForm");
                replyOpenForm.Text = this.ViewModel.ReplySubmitText;
                
                TextBox newReplyText = (TextBox)item.FindControl("NewReply");
                string parentPostId = currentPost.Id.ToString(CultureInfo.InvariantCulture);
                newReplyText.Attributes["parentPostId"] = parentPostId;
                newReplyText.Attributes["placeholder"] = this.ViewModel.ReplyPlaceholderText;

                Button newReplySubmit = (Button)item.FindControl("NewReplySubmit");
                newReplySubmit.Text = this.ViewModel.ReplySubmitText;
                newReplySubmit.CommandArgument = parentPostId;
            }
        }

        /// <summary>
        /// Fires when on of the NewReply buttons is clicked
        /// </summary>
        /// <param name="source">Origin of event</param>
        /// <param name="e">Event arguments</param>
        protected void PostRepeater_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName == "NewReply")
            {
                Button button = (Button)e.CommandSource;
                int parentPostId = int.Parse(e.CommandArgument.ToString(), CultureInfo.InvariantCulture);

                // Find the posted textbox value from the collection we stored at the beginning of Page_Load
                TextBox text = this.postbackTextBoxes.Single(box => box.Attributes["parentPostId"] == parentPostId.ToString(CultureInfo.InvariantCulture));

                this.ViewModel.AddReply(text.Text, parentPostId);
                text.Text = string.Empty;

                // We updated the contents, so we need to re-bind otherwise the refreshed page won't show our changes
                this.FetchAndBind();
            }
        }

        /// <summary>
        /// Builds the Html for rendering tags
        /// </summary>
        /// <param name="dataItem">The object with tags</param>
        /// <returns>The html of the tags</returns>
        protected string TagsHtml(object dataItem)
        {
            TaxonomyValueCollection tags = null;

            var wallPost = dataItem as WallPost;
            var wallReply = dataItem as WallReply;

            if (wallPost != null)
            {
                tags = wallPost.Tags;
            }
            else if (wallReply != null)
            {
                tags = wallReply.Tags;
            }

            return string.Join(" ", tags.Select(tag => "<span class=\"tag-label\">" + tag.Label + "</span>").ToArray());
        }

        private void FetchAndBind()
        {
            // Bind the parent repeater with data fetched from SharePoint
            this.PostRepeater.DataSource = this.ViewModel.Posts;
            this.PostRepeater.ItemDataBound += new RepeaterItemEventHandler(this.PostRepeater_ItemDataBound);
            this.PostRepeater.ItemCommand += new RepeaterCommandEventHandler(this.PostRepeater_ItemCommand);
            this.PostRepeater.DataBind();

            // Bind post submission
            this.NewPostSubmit.Click += new EventHandler(this.NewPostButton_Click);
        }
    }
}
