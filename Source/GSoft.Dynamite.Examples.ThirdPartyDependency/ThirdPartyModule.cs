using System.Reflection;
using Microsoft.SharePoint.Administration;

namespace ThirdPartyDependency
{
    /// <summary>
    /// An example third-party dependency
    /// </summary>
    public static class ThirdPartyModule
    {
        /// <summary>
        /// An example method that logs and returns a message containing the current assembly's version
        /// </summary>
        /// <returns>A string with the current Assembly Version in it.</returns>
        public static string HelloThirdParty()
        {
            string message = "Hello Third Party! My version is: " + Assembly.GetExecutingAssembly().FullName;

            SPDiagnosticsService.Local.WriteTrace(
                0,
                CreateDiagnosticCategory(TraceSeverity.Unexpected, EventSeverity.Error),
                TraceSeverity.Unexpected,
                message);

            return message;
        }

        private static SPDiagnosticsCategory CreateDiagnosticCategory(TraceSeverity traceSeverity, EventSeverity eventSeverity)
        {
            return new SPDiagnosticsCategory("Client.Project", traceSeverity, eventSeverity);
        }
    }
}
