using System;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.I2c;

namespace Rinsen.IoT.OneWire
{
    internal class I2cDeviceLocator
    {
        public async Task<I2cDevice> GetI2cDevice(byte address)
        {
            string aqs = I2cDevice.GetDeviceSelector();                     /* Get a selector string that will return all I2C controllers on the system */
            var dis = await DeviceInformation.FindAllAsync(aqs);            /* Find the I2C bus controller device with our selector string           */
            if (dis.Count == 0)
            {
                throw new InvalidOperationException("No I2C controllers were found on the system");
            }

            var settings = new I2cConnectionSettings(address);
            settings.BusSpeed = I2cBusSpeed.FastMode;

            return await I2cDevice.FromIdAsync(dis[0].Id, settings);    /* Create an I2cDevice with our selected bus controller and I2C settings */
        }
    }
}
