using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GSoft.Dynamite.Serializers;

namespace GSoft.Dynamite.UnitTests
{
    /// <summary>
    /// Base class that provides a serializer object factory method
    /// </summary>
    public class BaseSerializationTest
    {
        /// <summary>
        /// Factory method for serializers
        /// </summary>
        /// <returns>A new serializer instance</returns>
        public ISerializer GetSerializer()
        {
            return new JsonNetSerializer();
        }
    }
}
