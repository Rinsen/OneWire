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

        public static IEnumerable<T> GetDevices<T>(this IEnumerable<IOneWireDevice> devices) where T : IOneWireDevice
        {
            var result = new List<T>();
            foreach (var item in devices.Where(dev => dev.GetType().Equals(typeof(T))))
            {
                result.Add((T)item);
            }
            return result;
        }

    }
}
