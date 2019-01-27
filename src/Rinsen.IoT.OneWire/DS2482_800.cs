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
        private OneWireChannel _selectedChannel;

        public DS2482_800(I2cDevice i2cDevice, bool disposeI2cDevice)
            :base(i2cDevice, disposeI2cDevice)
        {
            _selectedChannel = OneWireChannel.Channel_IO0;
            Channels.Add(new DS2482Channel(this, OneWireChannel.Channel_IO0));
            Channels.Add(new DS2482Channel(this, OneWireChannel.Channel_IO1));
            Channels.Add(new DS2482Channel(this, OneWireChannel.Channel_IO2));
            Channels.Add(new DS2482Channel(this, OneWireChannel.Channel_IO3));
            Channels.Add(new DS2482Channel(this, OneWireChannel.Channel_IO4));
            Channels.Add(new DS2482Channel(this, OneWireChannel.Channel_IO5));
            Channels.Add(new DS2482Channel(this, OneWireChannel.Channel_IO6));
            Channels.Add(new DS2482Channel(this, OneWireChannel.Channel_IO7));
        }

        public override bool IsCorrectChannelSelected(OneWireChannel channel)
        {
            return _selectedChannel == channel;
        }

        public override void SetSelectedChannel(OneWireChannel channel)
        {
            _selectedChannel = channel;
        }
    }
}
