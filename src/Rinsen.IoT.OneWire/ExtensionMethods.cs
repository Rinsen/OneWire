using System.Collections.Generic;
using System.Linq;

namespace Rinsen.IoT.OneWire
{
    public static class ExtensionMethods
    {
        public static bool GetBit(this byte b, int bitNumber)
        {
            return (b & (1 << bitNumber)) != 0;
        }
    }
}
