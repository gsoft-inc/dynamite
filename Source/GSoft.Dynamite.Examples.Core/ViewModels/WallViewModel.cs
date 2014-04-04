using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using GSoft.Dynamite.Examples.Constants;
using GSoft.Dynamite.Examples.Entities;
using GSoft.Dynamite.Examples.Repositories;
using GSoft.Dynamite.Logging;
using GSoft.Dynamite.Taxonomy;
using GSoft.Dynamite.Utils;
using GSoft.Dynamite.ValueTypes;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.Examples.ViewModels
{
    /// <summary>
    /// View model for the Wall web part
    /// </summary>
    public class WallViewModel
    {
        private IWallPostRepository _wallPostRepository;
        private IWallReplyRepository _wallReplyRepository;
        private ITaxonomyService _taxonomyService;
        private IResourceLocator _resourceLocator;
        private ILogger _log;

        /// <summary>
        /// Constructor to inject the view model dependencies
        /// </summary>
        /// <param name="wallPostRepository">Wall post data access</param>
        /// <param name="wallReplyRepository">Wall reply data access</param>
        /// <param name="taxonomyService">Taxonomy service</param>
        /// <param name="resourceLocator">Resource utility</param>
        /// <param name="log">Logging utility</param>
        public WallViewModel(IWallPostRepository wallPostRepository, IWallReplyRepository wallReplyRepository, ITaxonomyService taxonomyService, IResourceLocator resourceLocator, ILogger log)
        {
            this._wallPostRepository = wallPostRepository;
            this._wallReplyRepository = wallReplyRepository;
            this._taxonomyService = taxonomyService;
            this._resourceLocator = resourceLocator;
            this._log = log;
        }

        /// <summary>
        /// The wall posts to display
        /// </summary>
        public IEnumerable<WallPost> Posts
        {
            get
            {
                var orderedPosts = this._wallPostRepository.AllWallPosts(SPContext.Current.Web).OrderByDescending(post => post.Created);

                this._log.Info(string.Format(CultureInfo.InvariantCulture, "{0} wall posts loaded", orderedPosts.Count()));

                return orderedPosts;
            }
        }

        /// <summary>
        /// Text to show on Post button
        /// </summary>
        public string PostSubmitText
        {
            get
            {
                return this._resourceLocator.Find("BasicWall_Post");
            }
        }

        /// <summary>
        /// Text to show in empty post form input
        /// </summary>
        public string PostPlaceholderText
        {
            get
            {
                return this._resourceLocator.Find("BasicWall_PostPlaceholder");                
            }
        }

        /// <summary>
        /// Text to show on Reply button
        /// </summary>
        public string ReplySubmitText
        {
            get
            {
                return this._resourceLocator.Find("BasicWall_Reply");
            }
        }

        /// <summary>
        /// Text to show in empty reply form input
        /// </summary>
        public string ReplyPlaceholderText
        {
            get
            {
                return this._resourceLocator.Find("BasicWall_ReplyPlaceholder");
            }
        }

        /// <summary>
        /// Creates a new post on the wall
        /// </summary>
        /// <param name="message">Text content of the post</param>
        /// <returns>The new WallPost object, null if something went wrong</returns>
        public WallPost AddPost(string message)
        {
            WallPost newPost = null;
            if (!string.IsNullOrEmpty(message))
            {
                newPost = new WallPost();
                newPost.Title = message.Length >= 255 ? message.Substring(0, 255) : message;
                newPost.Text = message;
                newPost.Author = new UserValue(SPContext.Current.Web.CurrentUser.ID);

                // Auto-tagging
                this.AddTags(message, newPost.Tags);

                this._wallPostRepository.Create(SPContext.Current.Web, newPost);
            }

            return newPost;
        }

        /// <summary>
        /// Creates a new reply to a wall post
        /// </summary>
        /// <param name="message">Text content of the reply</param>
        /// <param name="parentPostId">Id of the parent post</param>
        /// <returns>The new WallReply object, null if something went wrong</returns>
        public WallReply AddReply(string message, int parentPostId)
        {
            WallReply newReply = null;
            if (!string.IsNullOrEmpty(message))
            {
                newReply = new WallReply();
                newReply.Title = message.Length >= 255 ? message.Substring(0, 255) : message;
                newReply.Text = message;
                newReply.Author = new UserValue(SPContext.Current.Web.CurrentUser.ID);
                newReply.WallPost = new LookupValue(parentPostId);

                // Autotagging
                this.AddTags(message, newReply.Tags);

                this._wallReplyRepository.Create(SPContext.Current.Web, newReply);
            }

            return newReply;
        }

        private void AddTags(string message, TaxonomyValueCollection taxonomyValueCollection)
        {
            var hashTags = message.Split(' ').Where(token => token.StartsWith("#", StringComparison.OrdinalIgnoreCase)).Select(hashTag => hashTag.Remove(0, 1));
            foreach (string hashTag in hashTags)
            {
                var correspondingTaxonomyValue = this._taxonomyService.GetTaxonomyValueForLabel(SPContext.Current.Site, ProjectTaxonomy.TermStoreGroupName, ProjectTaxonomy.WallTermSetName, hashTag);
                if (correspondingTaxonomyValue != null)
                {
                    // A term with that label actually exists - use it as tag
                    taxonomyValueCollection.Add(correspondingTaxonomyValue);
                }
            }
        }
    }
}
