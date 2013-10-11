using System;

namespace GSoft.Dynamite.Extensions
{
    /// <summary>
    /// Extensions for SPSite and SPWeb that elevate privileges.
    /// Props to <c>http://solutionizing.net/2009/01/06/elegant-spsite-elevation/</c>
    /// </summary>
    [CLSCompliant(false)]
    public static class ElevationExtensions
    {
        /// <summary>
        /// Extension method that lets you access the system user token
        /// </summary>
        /// <param name="site">Self reference to site</param>
        /// <returns>The system token</returns>
        public static SPUserToken GetSystemToken(this SPSite site)
        {
            SPUserToken token = null;
            bool tempCADE = site.CatchAccessDeniedException;
            try
            {
                site.CatchAccessDeniedException = false;
                token = site.SystemAccount.UserToken;
            }
            catch (UnauthorizedAccessException)
            {
                SPSecurity.RunWithElevatedPrivileges(() =>
                {
                    using (SPSite elevSite = new SPSite(site.ID))
                    {
                        token = elevSite.SystemAccount.UserToken;
                    }
                });
            }
            finally
            {
                site.CatchAccessDeniedException = tempCADE;
            }

            return token;
        }

        /// <summary>
        /// Extension method that lets you runs code as system account
        /// </summary>
        /// <param name="site">Self reference to site</param>
        /// <param name="action">The delegate to execute</param>
        public static void RunAsSystem(this SPSite site, Action<SPSite> action)
        {
            using (SPSite elevSite = new SPSite(site.ID, site.GetSystemToken()))
            {
                action(elevSite);
            }
        }

        /// <summary>
        /// Extension method that lets you runs code as system account with a return value
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="site">Self reference to site</param>
        /// <param name="selector">The delegate to execute</param>
        /// <returns>A value of type T</returns>
        public static T SelectAsSystem<T>(this SPSite site, Func<SPSite, T> selector)
        {
            using (SPSite elevSite = new SPSite(site.ID, site.GetSystemToken()))
            {
                return selector(elevSite);
            }
        }

        /// <summary>
        /// Extension method that lets you runs code as system account on a specific web
        /// </summary>
        /// <param name="site">Self reference to site</param>
        /// <param name="webId">The target web for elevation</param>
        /// <param name="action">The delegate to execute</param>
        public static void RunAsSystem(this SPSite site, Guid webId, Action<SPWeb> action)
        {
            site.RunAsSystem(s =>
            {
                using (SPWeb elevatedWeb = s.OpenWeb(webId))
                {
                    action(elevatedWeb);
                }
            });
        }

        /// <summary>
        /// Extension method that lets you runs code as system account
        /// </summary>
        /// <param name="web">Self reference to the web</param>
        /// <param name="action">The delegate to execute</param>
        public static void RunAsSystem(this SPWeb web, Action<SPWeb> action)
        {
            web.Site.RunAsSystem(web.ID, action);
        }

        /// <summary>
        /// Extension method that lets you runs code as system account on a specific web with a return value
        /// </summary>
        /// <typeparam name="T">Type of the return value</typeparam>
        /// <param name="site">Self reference to the site</param>
        /// <param name="webId">The target web for elevation</param>
        /// <param name="selector">The delegate to execute</param>
        /// <returns>A value of type T</returns>
        public static T SelectAsSystem<T>(this SPSite site, Guid webId, Func<SPWeb, T> selector)
        {
            return site.SelectAsSystem(s =>
            {
                using (SPWeb elevatedWeb = s.OpenWeb(webId))
                {
                    return selector(elevatedWeb);
                }
            });
        }

        /// <summary>
        /// Extension method that lets you runs code as system account web with a return value
        /// </summary>
        /// <typeparam name="T">Type of the return value</typeparam>
        /// <param name="web">Self reference to the web</param>
        /// <param name="selector">The delegate to execute</param>
        /// <returns>A value of type T</returns>
        public static T SelectAsSystem<T>(this SPWeb web, Func<SPWeb, T> selector)
        {
            return web.Site.SelectAsSystem(web.ID, selector);
        }
    }
}
