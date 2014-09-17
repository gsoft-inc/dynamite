using System;
using System.Collections.Specialized;
using System.Linq;
using GSoft.Dynamite.Logging;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.Setup
{
    using Microsoft.SharePoint.BusinessData.Administration;

    /// <summary>
    /// List view builder.
    /// </summary>
    public class ViewBuilder
    {
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewBuilder"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public ViewBuilder(ILogger logger)
        {
            this.logger = logger;
        }

        /// <summary>
        /// Ensures the view.
        /// </summary>
        /// <param name="viewCollection">The view collection.</param>
        /// <param name="viewInfo">The view information.</param>
        /// <returns>The ensured view.</returns>
        public SPView EnsureView(SPViewCollection viewCollection, ViewInfo viewInfo)
        {
            var views = viewCollection.Cast<SPView>();
            var ensuredView = views.SingleOrDefault(view => view.Title.Equals(viewInfo.Name, StringComparison.OrdinalIgnoreCase));

            // If view collection doesn't already contain a view with the same name, create it
            if (ensuredView == null)
            {
                // Create view fields string collection
                var viewFields = new StringCollection();
                viewFields.AddRange(viewInfo.ViewFields);

                if (!string.IsNullOrEmpty(viewInfo.ProjectedFields) || !string.IsNullOrEmpty(viewInfo.Joins))
                {
                    ensuredView = viewCollection.Add(
                        viewInfo.Name,
                        viewFields,
                        viewInfo.Query,
                        viewInfo.Joins,
                        viewInfo.ProjectedFields,
                        viewInfo.RowLimit,
                        viewInfo.IsPaged,
                        viewInfo.IsDefaultView,
                        viewInfo.ViewType,
                        viewInfo.IsPersonalView);
                }
                else
                {
                    ensuredView = viewCollection.Add(
                        viewInfo.Name,
                        viewFields,
                        viewInfo.Query,
                        viewInfo.RowLimit,
                        viewInfo.IsPaged,
                        viewInfo.IsDefaultView,
                        viewInfo.ViewType,
                        viewInfo.IsPersonalView); 
                }

                viewCollection.List.Update();
                this.logger.Info(
                    "View '{0}' has been successfully created in list '{1}'.",
                    viewInfo.Name,
                    viewCollection.List.Title);
            }
            else
            {
                this.logger.Warn(
                    "View '{0}' has already been created in list '{1}'.  Updating the view.", 
                    viewInfo.Name, 
                    viewCollection.List.Title);

                // Update the existing view
                ensuredView.ViewFields.DeleteAll();
                viewInfo.ViewFields.ToList().ForEach(vf => ensuredView.ViewFields.Add(vf));
                ensuredView.Query = viewInfo.Query;
                ensuredView.Joins = viewInfo.Joins;
                ensuredView.ProjectedFields = viewInfo.ProjectedFields;
                ensuredView.RowLimit = viewInfo.RowLimit;
                ensuredView.Paged = viewInfo.IsPaged;
                ensuredView.DefaultView = viewInfo.IsDefaultView;
            }

            if (!string.IsNullOrEmpty(viewInfo.ViewData))
            {
                ensuredView.ViewData = viewInfo.ViewData; 
            }

            ensuredView.Update();

            return ensuredView;
        }
    }
}
