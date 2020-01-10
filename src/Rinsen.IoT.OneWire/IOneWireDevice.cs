using Rinsen.IoT.OneWire.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rinsen.IoT.OneWire
{
    public interface IOneWireDevice
    {
        byte[] OneWireAddress { get; }

        void Initialize(OneWireMaster oneWireMaster, byte[] oneWireAddress);
    }
}
