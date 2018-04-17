using System;

namespace Rinsen.IoT.OneWire
{
    public class DS2482800DeviceNotFoundException : DS2482DeviceNotFoundException
    {

        public DS2482800DeviceNotFoundException()
        {
        }

        public DS2482800DeviceNotFoundException(string message)
            : base(message)
        {
        }

        public DS2482800DeviceNotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
