using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSoft.Dynamite.Navigation
{
    /// <summary>
    /// The public sealed class Navigation Location
    /// </summary>
    public sealed class NavigationLocation
    {
        /// <summary>
        /// Refers to all items in the header
        /// </summary>
        public static readonly string Header = "Header";

        /// <summary>
        /// Refers to all items in the Main Menu
        /// </summary>
        public static readonly string MainMenu = "Main Menu";

        /// <summary>
        /// Refers to all items in the footer
        /// </summary>
        public static readonly string Footer = "Footer";

        /// <summary>
        /// Refers to all items in the Featured part
        /// </summary>
        public static readonly string Featured = "Featured";

        private NavigationLocation() 
        { 
        }
    }
}
