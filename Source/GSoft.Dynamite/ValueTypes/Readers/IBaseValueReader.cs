using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GSoft.Dynamite.ValueTypes.Readers
{
    public interface IBaseValueReader
    {
        Type AssociatedValueType { get; }
    }
}
