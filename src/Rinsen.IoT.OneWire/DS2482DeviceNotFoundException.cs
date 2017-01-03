using System;

namespace Rinsen.IoT.OneWire
{
    public class DS2482DeviceNotFoundException : Exception
    {
        public DS2482DeviceNotFoundException()
        {
        }

        public DS2482DeviceNotFoundException(string message) : base(message)
        {
        }

        public DS2482DeviceNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}