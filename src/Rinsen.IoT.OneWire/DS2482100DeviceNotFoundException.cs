using System;

namespace Rinsen.IoT.OneWire
{
    public class DS2482100DeviceNotFoundException : Exception
    {
        public DS2482100DeviceNotFoundException()
        {
        }

        public DS2482100DeviceNotFoundException(string message) : base(message)
        {
        }

        public DS2482100DeviceNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}