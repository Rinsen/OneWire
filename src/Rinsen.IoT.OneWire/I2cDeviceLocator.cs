using System;
using System.Linq;
using Windows.Devices.Enumeration;
using Windows.Devices.I2c;

namespace Rinsen.IoT.OneWire
{
    class I2cDeviceLocator
    {
        //const string I2C_CONTROLLER_NAME = "I2C5";        // For Minnowboard Max, use I2C5
        const string I2C_CONTROLLER_NAME = "I2C1";        // For Raspberry Pi 2, use I2C1

        private DeviceInformation _i2cBusController;

        public I2cDeviceLocator()
        {
            string aqs = I2cDevice.GetDeviceSelector(I2C_CONTROLLER_NAME);                     /* Get a selector string that will return all I2C controllers on the system */
            _i2cBusController = DeviceInformation.FindAllAsync(aqs).AsTask().Result.FirstOrDefault();  /* Find the I2C bus controller device with our selector string */

            if (_i2cBusController == default(DeviceInformation))
            {
                throw new InvalidOperationException("No I2C bus controllers were found on the system");
            }
        }

        public I2cDevice GetI2cDevice(byte address)
        {
            var settings = new I2cConnectionSettings(address);
            settings.BusSpeed = I2cBusSpeed.FastMode;

            return I2cDevice.FromIdAsync(_i2cBusController.Id, settings).AsTask().Result;    /* Create an I2cDevice with our selected bus controller and I2C settings */
        }
    }
}
