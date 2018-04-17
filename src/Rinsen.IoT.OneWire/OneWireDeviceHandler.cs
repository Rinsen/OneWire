using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Devices.I2c;

namespace Rinsen.IoT.OneWire
{
    public class OneWireDeviceHandler : IDisposable
    {
        private readonly List<DS2482> _ds2482Devices = new List<DS2482>();
        private readonly List<IOneWireDevice> _oneWireDevices = new List<IOneWireDevice>();
        private readonly Dictionary<byte, Type> _oneWireDeviceTypes = new Dictionary<byte, Type>();
        private readonly DS2482DeviceLocator _dS2482DeviceLocator = new DS2482DeviceLocator();

        /// <summary>
        /// One wire device handler without any connected DS2482 devices.<para />
        /// Connect devices with AddDS2482_100 or AddDS2482_800
        /// </summary>
        public OneWireDeviceHandler()
        {
            AddDeviceType<DS18S20>(0x10);
            AddDeviceType<DS18B20>(0x28);
        }

        /// <summary>
        /// One wire device handler via one single DS2482-100
        /// </summary>
        /// <param name="ad0">AD0 addess bit</param>
        /// <param name="ad1">AD1 addess bit</param>
        /// <exception cref="Rinsen.IoT.OneWire.DS2482DeviceNotFoundException">Thrown if no DS2482 device is detected</exception>
        public OneWireDeviceHandler(bool ad0 = true, bool ad1 = true)
            :this()
        {
            _ds2482Devices.Add(_dS2482DeviceLocator.CreateDS2482_100(ad0, ad1));
        }

        /// <summary>
        /// One wire device handler via one single DS2482-800
        /// </summary>
        /// <param name="ad0">AD0 addess bit</param>
        /// <param name="ad1">AD1 addess bit</param>
        /// <param name="ad2">AD2 addess bit</param>
        /// <exception cref="Rinsen.IoT.OneWire.DS2482DeviceNotFoundException">Thrown if no DS2482 device is detected</exception>
        public OneWireDeviceHandler(bool ad0, bool ad1, bool ad2)
            : this()
        {
            _ds2482Devices.Add(_dS2482DeviceLocator.CreateDS2482_800(ad0, ad1, ad2));
        }

        public void AddDS2482_100(bool ad0 = true, bool ad1 = true)
        {
            var dS2482Device = _dS2482DeviceLocator.CreateDS2482_100(ad0, ad1);

            _ds2482Devices.Add(dS2482Device);
        }

        public void AddDS2482_100(I2cDevice i2CDevice)
        {
            var dS2482Device = _dS2482DeviceLocator.CreateDS2482_100(i2CDevice);

            dS2482Device.EnableExternalI2cDeviceLifetimeControl();

            _ds2482Devices.Add(dS2482Device);
        }

        public void AddDS2482_800(bool ad0, bool ad1, bool ad2)
        {
            var dS2482Device = _dS2482DeviceLocator.CreateDS2482_800(ad0, ad1, ad2);

            _ds2482Devices.Add(dS2482Device);
        }

        public void AddDS2482_800(I2cDevice i2CDevice)
        {
            var dS2482Device = _dS2482DeviceLocator.CreateDS2482_800(i2CDevice);

            dS2482Device.EnableExternalI2cDeviceLifetimeControl();
            
            _ds2482Devices.Add(dS2482Device);
        }

        public void AddDeviceType<T>(byte familyCode) where T : IOneWireDevice
        {
            _oneWireDeviceTypes.Add(familyCode, typeof(T));
        }

        public IEnumerable<T> GetDevices<T>() where T : IOneWireDevice
        {
            if (!_oneWireDevices.Any())
            {
                foreach (var ds2482Device in _ds2482Devices)
                {
                    _oneWireDevices.AddRange(ds2482Device.GetConnectedOneWireDevices(_oneWireDeviceTypes));
                }
            }

            return _oneWireDevices.OfType<T>();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (var ds2482Device in _ds2482Devices)
                {
                    if (ds2482Device != null)
                        ds2482Device.Dispose();
                }
            }
        }
    }
}
