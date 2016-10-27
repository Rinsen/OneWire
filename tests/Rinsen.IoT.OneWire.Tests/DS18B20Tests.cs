using System;
using Xunit;

namespace Rinsen.IoT.OneWire.Tests
{
    public class DS18B20Tests
    {
        [Fact]
        public void TemperatureConversions()
        {
            var ds18b20 = new DS18B20();

            Assert.Equal(125, ds18b20.GetTemp_Read(0x07, 0xD0));
            Assert.Equal(85, ds18b20.GetTemp_Read(0x05, 0x50));
            Assert.Equal(25.0625, ds18b20.GetTemp_Read(0x01, 0x91));
            Assert.Equal(10.125, ds18b20.GetTemp_Read(0x00, 0xA2));
            Assert.Equal(0.5, ds18b20.GetTemp_Read(0x00, 0x08));
            Assert.Equal(0, ds18b20.GetTemp_Read(0x00, 0x00));
            Assert.Equal(-0.5, ds18b20.GetTemp_Read(0xFF, 0xF8));
            Assert.Equal(-10.125, ds18b20.GetTemp_Read(0xFF, 0x5E));
            Assert.Equal(-25.0625, ds18b20.GetTemp_Read(0xFE, 0x6F));
            Assert.Equal(-55, ds18b20.GetTemp_Read(0xFC, 0x90));
        }
    }
}
