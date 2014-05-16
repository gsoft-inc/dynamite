using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac.Builder;

namespace GSoft.Dynamite.ServiceLocator
{
    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public static class RegistrationExtensions
    {
        /// <summary>
        /// Share one instance of the component within the context of a single
        /// SharePoint site collection (SPSite).
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TStyle">Registration style.</typeparam>
        /// <typeparam name="TActivatorData">Activator data type.</typeparam>
        /// <param name="registration">The registration to configure.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public static IRegistrationBuilder<TLimit, TActivatorData, TStyle>
            InstancePerSite<TLimit, TActivatorData, TStyle>(
                this IRegistrationBuilder<TLimit, TActivatorData, TStyle> registration)
        {
            if (registration == null)
            {
                throw new ArgumentNullException("registration");
            }

            return registration.InstancePerMatchingLifetimeScope(SPLifetimeTag.Site);
        }

        /// <summary>
        /// Share one instance of the component within the context of a single
        /// SharePoint site (SPWeb).
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TStyle">Registration style.</typeparam>
        /// <typeparam name="TActivatorData">Activator data type.</typeparam>
        /// <param name="registration">The registration to configure.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public static IRegistrationBuilder<TLimit, TActivatorData, TStyle>
            InstancePerWeb<TLimit, TActivatorData, TStyle>(
                this IRegistrationBuilder<TLimit, TActivatorData, TStyle> registration)
        {
            if (registration == null)
            {
                throw new ArgumentNullException("registration");
            }

            return registration.InstancePerMatchingLifetimeScope(SPLifetimeTag.Web);
        }

        /// <summary>
        /// Share one instance of the component within the context of a single
        /// HTTP request (in a SharePoint context).
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TStyle">Registration style.</typeparam>
        /// <typeparam name="TActivatorData">Activator data type.</typeparam>
        /// <param name="registration">The registration to configure.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public static IRegistrationBuilder<TLimit, TActivatorData, TStyle>
            InstancePerRequest<TLimit, TActivatorData, TStyle>(
                this IRegistrationBuilder<TLimit, TActivatorData, TStyle> registration)
        {
            if (registration == null)
            {
                throw new ArgumentNullException("registration");
            }

            return registration.InstancePerMatchingLifetimeScope(SPLifetimeTag.Request);
        }
    }
}
