using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Devices.I2c;

namespace Rinsen.IoT.OneWire
{
    public abstract class DS2482 : IDisposable
    {
        protected readonly I2cDevice _i2cDevice;

        // global search state
        public byte[] ROM_NO { get; set; }
        protected int _lastDiscrepancy;
        protected int _lastFamilyDiscrepancy;
        protected bool _lastDeviceFlag;
        protected byte _crc8;

        public DS2482(I2cDevice i2cDevice)
        {
            _i2cDevice = i2cDevice;
        }

        public abstract List<IOneWireDevice> GetConnectedOneWireDevices(Dictionary<byte, Type> oneWireDeviceTypes);
        public abstract bool OneWireReset();
        public abstract void EnableStrongPullup();
        public abstract byte OneWireReadByte();
        public abstract bool OneWireReadBit();
        public abstract void OneWireWriteBit(bool bit_value);
        public abstract void OneWireWriteByte(byte byte_value);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_i2cDevice != null)
                    _i2cDevice.Dispose();
            }
        }

        static byte[] dscrc_table = (new[] {
        0, 94,188,226, 97, 63,221,131,194,156,126, 32,163,253, 31, 65,
      157,195, 33,127,252,162, 64, 30, 95,  1,227,189, 62, 96,130,220,
       35,125,159,193, 66, 28,254,160,225,191, 93,  3,128,222, 60, 98,
      190,224,  2, 92,223,129, 99, 61,124, 34,192,158, 29, 67,161,255,
       70, 24,250,164, 39,121,155,197,132,218, 56,102,229,187, 89,  7,
      219,133,103, 57,186,228,  6, 88, 25, 71,165,251,120, 38,196,154,
      101, 59,217,135,  4, 90,184,230,167,249, 27, 69,198,152,122, 36,
      248,166, 68, 26,153,199, 37,123, 58,100,134,216, 91,  5,231,185,
      140,210, 48,110,237,179, 81, 15, 78, 16,242,172, 47,113,147,205,
       17, 79,173,243,112, 46,204,146,211,141,111, 49,178,236, 14, 80,
      175,241, 19, 77,206,144,114, 44,109, 51,209,143, 12, 82,176,238,
       50,108,142,208, 83, 13,239,177,240,174, 76, 18,145,207, 45,115,
      202,148,118, 40,171,245, 23, 73,  8, 86,180,234,105, 55,213,139,
       87,  9,235,181, 54,104,138,212,149,203, 41,119,244,170, 72, 22,
      233,183, 85, 11,136,214, 52,106, 43,117,151,201, 74, 20,246,168,
      116, 42,200,150, 21, 75,169,247,182,232, 10, 84,215,137,107, 53}).Select(x => (byte)x).ToArray();

        //--------------------------------------------------------------------------
        // Calculate the CRC8 of the byte value provided with the current 
        // global 'crc8' value. 
        // Returns current global crc8 value
        //
        protected byte docrc8(byte value)
        {
            // See Application Note 27

            // TEST BUILD
            _crc8 = dscrc_table[_crc8 ^ value];
            return _crc8;
        }

        public class FunctionCommand
        {
            public const byte DEVICE_RESET = 0xF0;
            public const byte SET_READ_POINTER = 0xE1;
            public const byte WRITE_CONFIGURATION = 0xD2;
            public const byte ONEWIRE_RESET = 0xB4;
            public const byte ONEWIRE_SINGLE_BIT = 0x87;
            public const byte ONEWIRE_WRITE_BYTE = 0xA5;
            public const byte ONEWIRE_READ_BYTE = 0x96;
            public const byte ONEWIRE_TRIPLET = 0x78;
        }

        public class RegisterSelection
        {
            public const byte STATUS = 0xF0;
            public const byte READ_DATA = 0xE1;
            public const byte CONFIGURATION = 0xC3;
        }

        public class StatusBit
        {
            /// <summary>
            /// Branch Direction Taken (DIR)
            /// </summary>
            public const int BranchDirectionTaken = 7;
            /// <summary>
            /// Triplet Second Bit (TSB)
            /// </summary>
            public const int TripletSecondBit = 6;
            /// <summary>
            /// Single Bit Result (SBR)
            /// </summary>
            public const int SingleBitResult = 5;
            /// <summary>
            /// Device Reset (RST)
            /// </summary>
            public const int DeviceReset = 4;
            /// <summary>
            /// Logic Level (LL)
            /// </summary>
            public const int LogicLevel = 3;
            /// <summary>
            /// Short Detected (SD)
            /// </summary>
            public const int ShortDetected = 2;
            /// <summary>
            /// Presence-Pulse Detect (PPD)
            /// </summary>
            public const int PresencePulseDetected = 1;
            /// <summary>
            /// 1-Wire Busy (1WB)
            /// </summary>
            public const int OneWireBusy = 0;
        }

        public class TripletDirection
        {
            public const byte ONE = 0x40;
            public const byte ZERO = 0x00;
        }

    }
}
