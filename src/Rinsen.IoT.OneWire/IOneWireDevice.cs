using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rinsen.IoT.OneWire
{
    public interface IOneWireDevice
    {
        DS2482Channel DS2482Channel { get; }

        byte[] OneWireAddress { get; }

        void Initialize(DS2482Channel ds2482, byte[] oneWireAddress);
    }
}
