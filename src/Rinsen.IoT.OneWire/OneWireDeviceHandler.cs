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
        private readonly I2cDeviceLocator _i2cDeviceLocator;



        /// <summary>
        /// One wire device handler via DS2482 based one wire bridge
        /// </summary>
        /// <exception cref="Rinsen.IoT.OneWire.DS2482DeviceNotFoundException">Thrown if no DS2482 based device is detected</exception>
        public OneWireDeviceHandler(ConnectedDS2482 connectedDS2482)
        {
            if (!connectedDS2482.DS2482_100Devices.Any() && !connectedDS2482.DS2482_800Devices.Any())
                throw new ArgumentException("No DS2482 device address provided", nameof(connectedDS2482));

            _oneWireDeviceTypes = new Dictionary<byte, Type>();
            _oneWireDevices = new List<IOneWireDevice>();
            _ds2482Devices = new List<DS2482>();
            _i2cDeviceLocator = new I2cDeviceLocator();

            AddDeviceType<DS18S20>(0x10);
            AddDeviceType<DS18B20>(0x28);

            FindConnectedDs2482Devices(connectedDS2482);

            if (!_ds2482Devices.Any())
            {
                throw new DS2482DeviceNotFoundException("No DS2482 device detected, check that that the physical connection to the DS2482 one wire bridge is correct.");
            }
        }

        /// <summary>
        /// One wire device handler via one single DS2482-100
        /// </summary>
        /// <param name="ad0">AD0 addess bit</param>
        /// <param name="ad1">AD1 addess bit</param>
        /// <exception cref="Rinsen.IoT.OneWire.DS2482DeviceNotFoundException">Thrown if no DS2482-100 device is detected</exception>
        public OneWireDeviceHandler(bool ad0 = true, bool ad1 = true)
            :this(CreateConnectedDS2482_100(ad0, ad1))
        { }

        /// <summary>
        /// One wire device handler via one single DS2482-800
        /// </summary>
        /// <param name="ad0">AD0 addess bit</param>
        /// <param name="ad1">AD1 addess bit</param>
        /// <param name="ad2">AD2 addess bit</param>
        /// <exception cref="Rinsen.IoT.OneWire.DS2482DeviceNotFoundException">Thrown if no DS2482-800 device is detected</exception>
        public OneWireDeviceHandler(bool ad0 = true, bool ad1 = true, bool ad2 = true)
            : this(CreateConnectedDS2482_800(ad0, ad1, ad2))
        { }

        private void FindConnectedDs2482Devices(ConnectedDS2482 connectedDS2482)
        {
            foreach (var candidateAddress in connectedDS2482.DS2482_100Devices)
            {
                var i2cDevice = _i2cDeviceLocator.GetI2cDevice(candidateAddress);

                var ds2482_100 = new DS2482_100(i2cDevice);

                try
                {
                    ds2482_100.OneWireReset();

                    _ds2482Devices.Add(ds2482_100);
                }
                catch (Exception) // This could probably be done in a better way as it will hide the original exception...
                {
                }
            }

            foreach (var candidateAddress in connectedDS2482.DS2482_800Devices)
            {
                var i2cDevice = _i2cDeviceLocator.GetI2cDevice(candidateAddress);

                var ds2482_800 = new DS2482_800(i2cDevice, DS2482.SelectedChannel.Channel_IO0);

                try
                {
                    ds2482_800.OneWireReset();

                    _ds2482Devices.Add(ds2482_800);
                }
                catch (Exception) // This could probably be done in a better way as it will hide the original exception...
                {
                }
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

        private static ConnectedDS2482 CreateConnectedDS2482_100(bool ad0, bool ad1)
        {
            byte address = 0x18;
            if (ad0)
            {
                address |= 1 << 0;
            }
            if (ad1)
            {
                address |= 1 << 1;
            }

            var connected = new ConnectedDS2482();
            connected.DS2482_100Devices.Add(address);

            return connected;
        }

        private static ConnectedDS2482 CreateConnectedDS2482_800(bool ad0, bool ad1, bool ad2)
        {
            byte address = 0x18;
            if (ad0)
            {
                address |= 1 << 0;
            }
            if (ad1)
            {
                address |= 1 << 1;
            }
            if (ad1)
            {
                address |= 1 << 2;
            }

            var connected = new ConnectedDS2482();
            connected.DS2482_800Devices.Add(address);

            return connected;
        }
    }
}
