using Rinsen.IoT.OneWire.Abstractions;
using System;

namespace Rinsen.IoT.OneWire
{
    public class UndefinedOneWireDevice : IOneWireDevice
    {
        public byte[] OneWireAddress { get; private set; }

        public string OneWireAddressString { get { return BitConverter.ToString(OneWireAddress); } }

        public void Initialize(OneWireMaster oneWireMaster, byte[] oneWireAddress)
        {
            OneWireAddress = oneWireAddress;
        }
    }
}
