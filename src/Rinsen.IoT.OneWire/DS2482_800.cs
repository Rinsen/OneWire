using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Windows.Devices.I2c;
using static Rinsen.IoT.OneWire.DS2482Channel;

namespace Rinsen.IoT.OneWire
{
    

    public class DS2482_800 : DS2482
    {
        private readonly OneWireChannel _selectedChannel;

        public DS2482_800(I2cDevice i2cDevice)
            :base(i2cDevice)
        {
            _selectedChannel = OneWireChannel.Channel_IO0;
            Channels.Add(new DS2482Channel(i2cDevice, OneWireChannel.Channel_IO0));
            Channels.Add(new DS2482Channel(i2cDevice, OneWireChannel.Channel_IO1));
            Channels.Add(new DS2482Channel(i2cDevice, OneWireChannel.Channel_IO2));
            Channels.Add(new DS2482Channel(i2cDevice, OneWireChannel.Channel_IO3));
            Channels.Add(new DS2482Channel(i2cDevice, OneWireChannel.Channel_IO4));
            Channels.Add(new DS2482Channel(i2cDevice, OneWireChannel.Channel_IO5));
            Channels.Add(new DS2482Channel(i2cDevice, OneWireChannel.Channel_IO6));
            Channels.Add(new DS2482Channel(i2cDevice, OneWireChannel.Channel_IO7));
        }

        public override void EnsureCorrectChannel(OneWireChannel channel)
        {
            if (_selectedChannel == channel)
            {
                return;
            }
            throw new NotImplementedException();
        }
    }
}
