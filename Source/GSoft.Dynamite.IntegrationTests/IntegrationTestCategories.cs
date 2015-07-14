using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSoft.Dynamite.IntegrationTests
{
    /// <summary>
    /// Test categories to segment test runs
    /// </summary>
    public class IntegrationTestCategories
    {
        /// <summary>
        /// Sanity-check level tests (i.e. filters for the most minimal test suite possible)
        /// </summary>
        public const string Sanity = "SANITY";

        /// <summary>
        /// Integration tests that require Fakes (and thus will only run under VSTest and will
        /// break any MSTest.exe-based CI build runner)
        /// </summary>
        public const string RequiresFakes = "REQUIRES_FAKES";
    }
}
