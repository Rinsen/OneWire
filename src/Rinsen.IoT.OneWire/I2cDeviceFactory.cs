using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.I2c;

namespace Rinsen.IoT.OneWire
{
    internal class I2cDeviceFactory
    {
        //const string I2C_CONTROLLER_NAME = "I2C5";        // For Minnowboard Max, use I2C5
        private const string I2C_CONTROLLER_NAME = "I2C1";        // For Raspberry Pi 2, use I2C1

        public async Task<I2cDevice> GetI2cDeviceAsync(byte address)
        {
            string aqs = I2cDevice.GetDeviceSelector(I2C_CONTROLLER_NAME);                     /* Get a selector string that will return all I2C controllers on the system */
            var _i2cBusControllers = await DeviceInformation.FindAllAsync(aqs);  /* Find the I2C bus controller device with our selector string */

            if (!_i2cBusControllers.Any())
            {
                throw new InvalidOperationException("No I2C bus controllers were found on the system");
            }

            var settings = new I2cConnectionSettings(address)
            {
                BusSpeed = I2cBusSpeed.FastMode
            };

            return await I2cDevice.FromIdAsync(_i2cBusControllers.First().Id, settings);    /* Create an I2cDevice with our selected bus controller and I2C settings */
        }
    }
}
