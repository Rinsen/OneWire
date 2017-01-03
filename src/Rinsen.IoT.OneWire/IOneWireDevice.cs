using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rinsen.IoT.OneWire
{
    public interface IOneWireDevice
    {
        DS2482 DS2482 { get; }

        byte[] OneWireAddress { get; }

        void Initialize(DS2482 ds2482, byte[] oneWireAddress);
    }
}
