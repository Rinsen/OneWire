using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rinsen.IoT.OneWire
{
    public class ConnectedDS2482
    {

        public List<byte> DS2482_100Devices { get; set; } = new List<byte>();
        public List<byte> DS2482_800Devices { get; set; } = new List<byte>();
    }
}
