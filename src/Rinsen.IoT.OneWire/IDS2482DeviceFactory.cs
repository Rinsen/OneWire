using System.Threading.Tasks;
using Windows.Devices.I2c;

namespace Rinsen.IoT.OneWire
{
    public interface IDS2482DeviceFactory
    {
        Task<DS2482_100> CreateDS2482_100(bool ad0, bool ad1);
        DS2482_100 CreateDS2482_100(I2cDevice i2cDevice);
        Task<DS2482_800> CreateDS2482_800(bool ad0, bool ad1, bool ad2);
        DS2482_800 CreateDS2482_800(I2cDevice i2cDevice);
    }
}