using System;

namespace Rinsen.IoT.OneWire
{
    public class UndefinedOneWireDevice : IOneWireDevice
    {
        public UndefinedOneWireDevice(DS2482 ds2482, byte[] oneWireAddress)
        {
            DS2482 = ds2482;
            OneWireAddress = oneWireAddress;
        }

        public DS2482 DS2482 { get; }

        public byte[] OneWireAddress { get; }

        public string OneWireAddressString { get { return BitConverter.ToString(OneWireAddress); } }

        public void Initialize(DS2482 ds2482, byte[] oneWireAddress)
        {
            throw new NotImplementedException();
        }
    }
}
