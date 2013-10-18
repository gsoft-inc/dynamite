using ThirdPartyDependency;

namespace GSoft.Dynamite.Examples.Core.Utilities
{
    /// <summary>
    /// Example of dependency usage
    /// </summary>
    public static class DependencyExample
    {
        /// <summary>
        /// Uses a third party to output and log a message showing the third party's version number
        /// </summary>
        /// <returns>A message from the third party including its Assembly Version</returns>
        public static string HelloDependency()
        {
            return ThirdPartyModule.HelloThirdParty();
        }
    }
}
