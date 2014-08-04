using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.Configuration
{
    /// <summary>
    /// Configuration Interface. Describe the contract to implement to be able to get some configuration data.
    /// </summary>
    public interface IConfiguration
    {
        /// <summary>
        /// Method to get a configuration value with a specific Key
        /// </summary>
        /// <param name="web">The current web</param>
        /// <param name="key">A key to retrieve the value</param>
        /// <returns>A serialized version of the value</returns>
        /// <remarks>
        /// The implementation of this method should check on the most nested scope first than fallback on the next.
        /// Web > Site > WebApplication > Farm
        /// </remarks>
        string GetByKeyByMostNestedScope(SPWeb web, string key);

        /// <summary>
        /// Method to get the Mail to send exception and errors
        /// </summary>
        /// <param name="web">The current web</param>
        /// <returns>Comma seperated emails</returns>
        /// <remarks>
        /// The implementation of this method should check on the most nested scope first than fallback on the next.
        /// Web > Site > WebApplication > Farm
        /// </remarks>
        string GetErrorEmailByMostNestedScope(SPWeb web);
    }
}
