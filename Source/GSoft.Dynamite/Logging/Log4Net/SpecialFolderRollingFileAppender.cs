using System;
using System.Text.RegularExpressions;
using log4net.Appender;

namespace GSoft.Dynamite.Sharepoint2013.Logging.Log4Net
{
    /// <summary>
    /// RollingFileAppender which transforms special folders in the file path to their real representations.
    /// </summary>
    public sealed class SpecialFolderRollingFileAppender : RollingFileAppender
    {
        #region Fields

        private static readonly Regex SpecialTokenRegex = new Regex(@"\$\(([^)]+)\)", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant);

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the path to the file that logging will be written to.
        /// </summary>
        public override string File
        {
            get
            {
                return base.File;
            }

            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    base.File = SpecialFolderRollingFileAppender.ParseAndReplaceSpecialFolders(value);
                }
                else
                {
                    base.File = value;
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Takes a path and replaces all SpecialFolder tokens with their actual representations.
        /// </summary>
        /// <param name="path">
        /// The path to parse and replace.
        /// </param>
        /// <returns>
        /// The path with the replaced special folders.
        /// </returns>
        public static string ParseAndReplaceSpecialFolders(string path)
        {
            foreach (Match m in SpecialFolderRollingFileAppender.SpecialTokenRegex.Matches(path))
            {
                var token = m.Groups[1].Value;

                Environment.SpecialFolder folder;
                if (Enum.TryParse(token, true, out folder))
                {
                    path = path.Replace(m.Value, Environment.GetFolderPath(folder));
                }
            }

            return path;
        }

        #endregion
    }
}