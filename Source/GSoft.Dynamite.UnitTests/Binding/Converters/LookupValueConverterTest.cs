using System;
using GSoft.Dynamite.Binding;
using GSoft.Dynamite.Binding.Converters;
using GSoft.Dynamite.Logging;
using GSoft.Dynamite.ValueTypes;
using Microsoft.SharePoint;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GSoft.Dynamite.UnitTests.Binding.Converters
{
    /// <summary>
    /// Test class
    /// </summary>
    [TestClass]
    public class LookupValueConverterTest
    {
        /// <summary>
        /// Test method
        /// </summary>
        [TestMethod]
        public void LookupValueConverter_TestThatLookupValueIsProperlyInitializedWhenIdAndValueAreSplittedByKey()
        {
            var converter = new LookupValueConverter(new TraceLogger("Logger", "Test", true));

            var converted = converter.Convert("2;#test111", null);

            var lookup = converted as LookupValue;

            if (lookup == null)
            {
                Assert.Fail("Lookup is null");
            }

            Assert.AreEqual(2, lookup.Id);
            Assert.AreEqual("test111", lookup.Value);
        }
    }
}
