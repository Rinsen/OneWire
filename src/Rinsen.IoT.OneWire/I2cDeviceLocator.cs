using System;
using Windows.Devices.Enumeration;
using Windows.Devices.I2c;

namespace Rinsen.IoT.OneWire
{
    class I2cDeviceLocator
    {
        //const string I2C_CONTROLLER_NAME = "I2C5";        // For Minnowboard Max, use I2C5
        const string I2C_CONTROLLER_NAME = "I2C1";        // For Raspberry Pi 2, use I2C1

        public I2cDevice GetI2cDevice(byte address)
        {
            string aqs = I2cDevice.GetDeviceSelector(I2C_CONTROLLER_NAME);                     /* Get a selector string that will return all I2C controllers on the system */
            var deviceInformation = DeviceInformation.FindAllAsync(aqs).AsTask().Result;            /* Find the I2C bus controller device with our selector string           */
            if (deviceInformation.Count == 0)
            {
                throw new InvalidOperationException("No I2C controllers were found on the system");
            }

            var settings = new I2cConnectionSettings(address);
            settings.BusSpeed = I2cBusSpeed.FastMode;

            var count = deviceInformation.Count;
            var devAdd = deviceInformation[0];

            return I2cDevice.FromIdAsync(devAdd.Id, settings).AsTask().Result;    /* Create an I2cDevice with our selected bus controller and I2C settings */
        }
    }
}
