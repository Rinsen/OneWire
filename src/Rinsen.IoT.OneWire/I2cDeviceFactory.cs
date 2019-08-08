using System;
using System.Device.I2c;
using System.Linq;
using System.Threading.Tasks;

namespace Rinsen.IoT.OneWire
{
    internal class I2cDeviceFactory
    {
        //const string I2C_CONTROLLER_NAME = "I2C5";        // For Minnowboard Max, use I2C5
        private const string I2C_CONTROLLER_NAME = "I2C1";        // For Raspberry Pi 2, use I2C1

        public I2cDevice GetI2cDevice(byte address)
        {
            //string aqs = I2cDevice.GetDeviceSelector(I2C_CONTROLLER_NAME);                     /* Get a selector string that will return all I2C controllers on the system */
            //var _i2cBusControllers = await DeviceInformation.FindAllAsync(aqs);  /* Find the I2C bus controller device with our selector string */

            //if (!_i2cBusControllers.Any())
            //{
            //    throw new InvalidOperationException("No I2C bus controllers were found on the system");
            //}

            //var settings = new I2cConnectionSettings(address)
            //{
            //    BusSpeed = I2cBusSpeed.FastMode
            //};

            //return await I2cDevice.FromIdAsync(_i2cBusControllers.First().Id, settings);    /* Create an I2cDevice with our selected bus controller and I2C settings */

            var i2cConnectionSettings = new I2cConnectionSettings(1, address);

            return I2cDevice.Create(i2cConnectionSettings);
        }
    }
}


//public Windows10I2cDevice(I2cConnectionSettings settings)
//{
//    _settings = settings;
//    var winSettings = new WinI2c.I2cConnectionSettings(settings.DeviceAddress);

//    string busFriendlyName = $"I2C{settings.BusId}";
//    string deviceSelector = WinI2c.I2cDevice.GetDeviceSelector(busFriendlyName);

//    DeviceInformationCollection deviceInformationCollection = DeviceInformation.FindAllAsync(deviceSelector).WaitForCompletion();
//    if (deviceInformationCollection.Count == 0)
//    {
//        throw new ArgumentException($"No I2C device exists for bus ID {settings.BusId}.", $"{nameof(settings)}.{nameof(settings.BusId)}");
//    }

//    _winI2cDevice = WinI2c.I2cDevice.FromIdAsync(deviceInformationCollection[0].Id, winSettings).WaitForCompletion();
//    if (_winI2cDevice == null)
//    {
//        throw new PlatformNotSupportedException($"I2C devices are not supported.");
//    }
//}