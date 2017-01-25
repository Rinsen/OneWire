using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Devices.I2c;
using static Rinsen.IoT.OneWire.DS2482Channel;

namespace Rinsen.IoT.OneWire
{
    public abstract class DS2482 : IDisposable
    {
        protected IList<DS2482Channel> Channels;
        protected readonly I2cDevice _i2cDevice;

        public DS2482(I2cDevice i2cDevice)
        {
            _i2cDevice = i2cDevice;
        }

        public abstract void EnsureCorrectChannel(OneWireChannel channel);

        public List<IOneWireDevice> GetConnectedOneWireDevices(Dictionary<byte, Type> oneWireDeviceTypes)
        {
            var oneWireDevices = new List<IOneWireDevice>();

            foreach (var channel in Channels)
            {
                oneWireDevices.AddRange(channel.GetConnectedOneWireDevices(oneWireDeviceTypes));
            }

            return oneWireDevices;
        }

        public bool OneWireReset()
        {
            return Channels.First().OneWireReset();
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
                if (_i2cDevice != null)
                    _i2cDevice.Dispose();
            }
        }
    }
}
