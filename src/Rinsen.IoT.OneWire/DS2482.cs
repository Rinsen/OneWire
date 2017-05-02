using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Devices.I2c;
using static Rinsen.IoT.OneWire.DS2482Channel;

namespace Rinsen.IoT.OneWire
{
    public abstract class DS2482 : IDisposable
    {
        protected IList<DS2482Channel> Channels = new List<DS2482Channel>();
        public I2cDevice I2cDevice { get; }
        public DS2482(I2cDevice i2cDevice)
        {
            I2cDevice = i2cDevice;
        }

        public abstract bool IsCorrectChannelSelected(OneWireChannel channel);

        public abstract void SetSelectedChannel(OneWireChannel channel);

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
                if (I2cDevice != null)
                    I2cDevice.Dispose();
            }
        }
    }
}
