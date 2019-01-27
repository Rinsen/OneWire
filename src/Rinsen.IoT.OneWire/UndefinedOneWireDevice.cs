using System;

namespace Rinsen.IoT.OneWire
{
    public class UndefinedOneWireDevice : IOneWireDevice
    {
        public UndefinedOneWireDevice(DS2482Channel ds2482Channel, byte[] oneWireAddress)
        {
            DS2482Channel = ds2482Channel;
            OneWireAddress = oneWireAddress;
        }

        public DS2482Channel DS2482Channel { get; }

        public byte[] OneWireAddress { get; }

        public string OneWireAddressString { get { return BitConverter.ToString(OneWireAddress); } }

        public void Initialize(DS2482Channel ds2482, byte[] oneWireAddress)
        {
            throw new NotImplementedException();
        }
    }
}
