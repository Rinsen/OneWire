using System;
using System.Collections.Generic;
using System.Linq;

namespace Rinsen.IoT.OneWire
{
    public class OneWireDeviceHandler : IDisposable
    {
        private readonly List<DS2482> _ds2482Devices;
        private readonly List<IOneWireDevice> _oneWireDevices;
        private readonly Dictionary<byte, Type> _oneWireDeviceTypes;

        /// <summary>
        /// One wire device handler via DS2482 based one wire bridge
        /// </summary>
        /// <param name="ad0">AD0 addess bit</param>
        /// <param name="ad1">AD1 addess bit</param>
        /// <exception cref="Rinsen.IoT.OneWire.DS2482DeviceNotFoundException">Thrown if no DS2482 based device is detected</exception>
        public OneWireDeviceHandler(bool ad0 = true, bool ad1 = true)
        {

            _oneWireDeviceTypes = new Dictionary<byte, Type>();
            _oneWireDevices = new List<IOneWireDevice>();
            _ds2482Devices = new List<DS2482>();

            AddDeviceType<DS18S20>(0x10);
            AddDeviceType<DS18B20>(0x28);

            byte address = 0x18;
            if (ad0)
            {
                address |= 1 << 0;
            }
            if (ad1)
            {
                address |= 1 << 1;
            }
            
            var ds2482_100 = new DS2482_100(new I2cDeviceLocator().GetI2cDevice(address));

            try
            {
                ds2482_100.OneWireReset();

                _ds2482Devices.Add(ds2482_100);
            }
            catch (Exception e)
            {
                throw new DS2482DeviceNotFoundException("No DS2482 device detected, check that AD0 and AD1 is correct in ctor and that the physical connection to the DS2482 one wire bridge is correct.", e);
            }
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
