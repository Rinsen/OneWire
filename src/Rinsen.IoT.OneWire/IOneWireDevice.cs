using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rinsen.IoT.OneWire
{
    public interface IOneWireDevice
    {
        DS2482_100 DS2482_100 { get; set; }

        byte[] OneWireAddress { get; set; }

        void Initialize();
    }
}
