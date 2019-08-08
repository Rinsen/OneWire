using System.Device.I2c;
using System.Threading.Tasks;

namespace Rinsen.IoT.OneWire
{
    public interface IDS2482DeviceFactory
    {
        DS2482_100 CreateDS2482_100(bool ad0, bool ad1);
        DS2482_100 CreateDS2482_100(I2cDevice i2cDevice);
        DS2482_800 CreateDS2482_800(bool ad0, bool ad1, bool ad2);
        DS2482_800 CreateDS2482_800(I2cDevice i2cDevice);
    }
}