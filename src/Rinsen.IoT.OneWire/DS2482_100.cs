using System;
using System.Diagnostics;
using System.Linq;
using Windows.Devices.I2c;

namespace Rinsen.IoT.OneWire
{
    //
    // This implementation is based on Maxim sample implementation and has parts from converted C code
    // source: http://www.maximintegrated.com/en/app-notes/index.mvp/id/187
    //
    //

    public class DS2482_100 : IDisposable
    {
        readonly I2cDevice _i2cDevice;

        // Enable leaky abstraction
        // public I2cDevice I2CDevice { get { return _i2cDevice; } }

        public DS2482_100(I2cDevice i2cDevice)
        {
            _i2cDevice = i2cDevice;
            ROM_NO = new byte[8];
        }


        // global search state
        public byte[] ROM_NO { get; set; }
        int LastDiscrepancy;
        int LastFamilyDiscrepancy;
        bool LastDeviceFlag;
        byte crc8;

        /// <summary>
        /// Find the 'first' devices on the 1-Wire bus
        /// </summary>
        /// <returns>true : device found, ROM number in ROM_NO buffer, false : no device present</returns>
        public bool OneWireFirst()
        {
            // reset the search state
            LastDiscrepancy = 0;
            LastDeviceFlag = false;
            LastFamilyDiscrepancy = 0;

            return OneWireSearch();
        }

        /// <summary>
        /// Find the 'next' devices on the 1-Wire bus
        /// </summary>
        /// <returns>true : device found, ROM number in ROM_NO buffer, false : device not found, end of search</returns>
        public bool OnoWireNext()
        {
            // leave the search state alone
            return OneWireSearch();
        }

        /// <summary>
        /// Perform the 1-Wire Search Algorithm on the 1-Wire bus using the existing search state.
        /// </summary>
        /// <returns>true : device found, ROM number in ROM_NO buffer, false : device not found, end of search</returns>
        public bool OneWireSearch()
        {
            int id_bit_number;
            int last_zero, rom_byte_number;
            bool id_bit, cmp_id_bit, search_direction, search_result;
            byte rom_byte_mask;

            // initialize for search
            id_bit_number = 1;
            last_zero = 0;
            rom_byte_number = 0;
            rom_byte_mask = 1;
            search_result = false;
            crc8 = 0;

            // if the last call was not the last one
            if (!LastDeviceFlag)
            {
                // 1-Wire reset
                if (!OneWireReset())
                {
                    // reset the search
                    LastDiscrepancy = 0;
                    LastDeviceFlag = false;
                    LastFamilyDiscrepancy = 0;
                    return false;
                }

                // issue the search command 
                OneWireWriteByte(0xF0);

                // loop to do the search
                do
                {
                    // read a bit and its complement
                    id_bit = OneWireReadBit();
                    cmp_id_bit = OneWireReadBit();

                    // check for no devices on 1-wire
                    if (id_bit && cmp_id_bit)
                        break;
                    else
                    {
                        // all devices coupled have 0 or 1
                        if (id_bit != cmp_id_bit)
                            search_direction = id_bit;  // bit write value for search
                        else
                        {
                            // if this discrepancy if before the Last Discrepancy
                            // on a previous next then pick the same as last time
                            if (id_bit_number < LastDiscrepancy)
                                search_direction = ((ROM_NO[rom_byte_number] & rom_byte_mask) > 0);
                            else
                                // if equal to last pick 1, if not then pick 0
                                search_direction = (id_bit_number == LastDiscrepancy);

                            // if 0 was picked then record its position in LastZero
                            if (!search_direction)
                            {
                                last_zero = id_bit_number;

                                // check for Last discrepancy in family
                                if (last_zero < 9)
                                    LastFamilyDiscrepancy = last_zero;
                            }
                        }

                        // set or clear the bit in the ROM byte rom_byte_number
                        // with mask rom_byte_mask
                        if (search_direction)
                            ROM_NO[rom_byte_number] |= rom_byte_mask;
                        else
                        {
                            var result = (byte)~rom_byte_mask;
                            ROM_NO[rom_byte_number] &= result;
                        }
                            

                        // serial number search direction write bit
                        OneWireWriteBit(search_direction);

                        // increment the byte counter id_bit_number
                        // and shift the mask rom_byte_mask
                        id_bit_number++;
                        rom_byte_mask <<= 1;

                        // if the mask is 0 then go to new SerialNum byte rom_byte_number and reset mask
                        if (rom_byte_mask == 0)
                        {
                            docrc8(ROM_NO[rom_byte_number]);  // accumulate the CRC
                            rom_byte_number++;
                            rom_byte_mask = 1;
                        }
                    }
                }
                while (rom_byte_number < 8);  // loop until through all ROM bytes 0-7

                // if the search was successful then
                if (!((id_bit_number < 65) || (crc8 != 0)))
                {
                    // search successful so set LastDiscrepancy,LastDeviceFlag,search_result
                    LastDiscrepancy = last_zero;

                    // check for last device
                    if (LastDiscrepancy == 0)
                        LastDeviceFlag = true;

                    search_result = true;
                }
            }

            // if no device found then reset counters so next 'search' will be like a first
            if (!search_result || ROM_NO[0] == 0)
            {
                LastDiscrepancy = 0;
                LastDeviceFlag = false;
                LastFamilyDiscrepancy = 0;
                search_result = false;
            }

            return search_result;
        }

        /// <summary>
        /// Verify the device with the ROM number in ROM_NO buffer is present.
        /// </summary>
        /// <returns>true : device verified present, false : device not present</returns>
        public bool OneWireVerify()
        {
            byte[] rom_backup = new byte[8];
            int i, ld_backup, lfd_backup;
            bool ldf_backup, rslt;

            // keep a backup copy of the current state
            for (i = 0; i < 8; i++)
                rom_backup[i] = ROM_NO[i];
            ld_backup = LastDiscrepancy;
            ldf_backup = LastDeviceFlag;
            lfd_backup = LastFamilyDiscrepancy;

            // set search to find the same device
            LastDiscrepancy = 64;
            LastDeviceFlag = false;

            if (OneWireSearch())
            {
                // check if same device found
                rslt = true;
                for (i = 0; i < 8; i++)
                {
                    if (rom_backup[i] != ROM_NO[i])
                    {
                        rslt = false;
                        break;
                    }
                }
            }
            else
                rslt = false;

            // restore the search state 
            for (i = 0; i < 8; i++)
                ROM_NO[i] = rom_backup[i];
            LastDiscrepancy = ld_backup;
            LastDeviceFlag = ldf_backup;
            LastFamilyDiscrepancy = lfd_backup;

            // return the result of the verify
            return rslt;
        }

        /// <summary>
        /// Setup the search to find the device type 'family_code' on the next call to OWNext() if it is present.
        /// </summary>
        /// <param name="family_code"></param>
        public void OneWireTargetSetup(byte family_code)
        {
            int i;

            // set the search state to find SearchFamily type devices
            ROM_NO[0] = family_code;
            for (i = 1; i < 8; i++)
                ROM_NO[i] = 0;
            LastDiscrepancy = 64;
            LastFamilyDiscrepancy = 0;
            LastDeviceFlag = false;
        }

        /// <summary>
        /// Setup the search to skip the current device type on the next call to OWNext().
        /// </summary>
        public void OneWireFamilySkipSetup()
        {
            // set the Last discrepancy to last family discrepancy
            LastDiscrepancy = LastFamilyDiscrepancy;
            LastFamilyDiscrepancy = 0;

            // check for end of list
            if (LastDiscrepancy == 0)
                LastDeviceFlag = true;
        }

        //--------------------------------------------------------------------------
        // 1-Wire Functions to be implemented for a particular platform
        //--------------------------------------------------------------------------

        /// <summary>
        /// Reset the 1-Wire bus and return the presence of any device
        /// </summary>
        /// <returns>true : device present, false : no device present</returns>
        public bool OneWireReset()
        {
            // platform specific
            // TMEX API TEST BUILD
            //return (TMTouchReset(session_handle) == 1);

            _i2cDevice.Write(new byte[] { FunctionCommand.ONEWIRE_RESET });

            var status = ReadStatus();

            if (status.GetBit(StatusBit.ShortDetected))
            {
                throw new InvalidOperationException("One Wire short detected");
            }

            return status.GetBit(StatusBit.PresencePulseDetected);
        }

        /// <summary>
        /// Read status byte from DS2482_100
        /// </summary>
        /// <param name="setReadPointerToStatus">Set to true if read pointer should be moved to status register before reading status</param>
        /// <returns></returns>
        public byte ReadStatus(bool setReadPointerToStatus = false)
        {
            var statusBuffer = new byte[1];
            if (setReadPointerToStatus)
            {
                _i2cDevice.WriteRead(new byte[] { FunctionCommand.SET_READ_POINTER, RegisterSelection.STATUS }, statusBuffer);
            }
            else
            {
                _i2cDevice.Read(statusBuffer);
            }

            if (statusBuffer.Length < 1)
            {
                throw new InvalidOperationException("Read status failed");
            }

            var stopWatch = new Stopwatch();
            do
            {
                if (stopWatch.ElapsedMilliseconds > 1)
                {
                    throw new InvalidOperationException("One Wire bus busy for too long");
                }
                _i2cDevice.Read(statusBuffer);
            } while (statusBuffer[0].GetBit(StatusBit.OneWireBusy));

            return statusBuffer[0];
        }

        void WaitForOneWireReady()
        {
            var status = new byte[1];
            var stopWatch = new Stopwatch();
            do
            {
                if (stopWatch.ElapsedMilliseconds > 5000)
                {
                    throw new InvalidOperationException("One Wire bus busy for too long");
                }
                _i2cDevice.WriteRead(new byte[] { FunctionCommand.SET_READ_POINTER, RegisterSelection.STATUS }, status);
            } while (status[0].GetBit(StatusBit.OneWireBusy));
        }

        /// <summary>
        /// Send 8 bits of data to the 1-Wire bus
        /// </summary>
        /// <param name="byte_value">byte to send</param>
        public void OneWireWriteByte(byte byte_value)
        {
            // platform specific

            // TMEX API TEST BUILD
            //TMTouchByte(session_handle, byte_value);

            _i2cDevice.Write(new byte[] { FunctionCommand.ONEWIRE_WRITE_BYTE, byte_value });

            ReadStatus();
        }

        /// <summary>
        /// Send 1 bit of data to teh 1-Wire bus 
        /// </summary>
        /// <param name="bit_value"></param>
        public void OneWireWriteBit(bool bit_value)
        {
            // platform specific

            // TMEX API TEST BUILD
            //TMTouchBit(session_handle, (short)bit_value);

            var byte_value = new byte();
            if (bit_value)
            {
                byte_value |= 1 << 7;
            }
            
            _i2cDevice.Write(new byte[] { FunctionCommand.ONEWIRE_SINGLE_BIT, byte_value });


            ReadStatus();
        }

        /// <summary>
        /// Read 1 bit of data from the 1-Wire bus 
        /// </summary>
        /// <returns>true : bit read is 1, false : bit read is 0</returns>
        public bool OneWireReadBit()
        {
            // platform specific

            // TMEX API TEST BUILD
            //return (byte)TMTouchBit(session_handle, 0x01);

            var byte_value = new byte();

            byte_value |= 1 << 7;

            _i2cDevice.Write(new[] { FunctionCommand.ONEWIRE_SINGLE_BIT, byte_value });

            var status = ReadStatus();

            return status.GetBit(StatusBit.SingleBitResult);
        }

        /// <summary>
        /// Read 1 bit of data from the 1-Wire bus 
        /// </summary>
        /// <returns>true : bit read is 1, false : bit read is 0</returns>
        public byte OneWireReadByte()
        {
            var buffer = new byte[1];
            _i2cDevice.Write(new byte[] { DS2482_100.FunctionCommand.ONEWIRE_READ_BYTE });
            ReadStatus();
            _i2cDevice.WriteRead(new byte[] { DS2482_100.FunctionCommand.SET_READ_POINTER, DS2482_100.RegisterSelection.READ_DATA }, buffer);
            return buffer[0];
        }

        public void EnableStrongPullup()
        {
            var configuration = new byte();
            configuration |= 1 << 2;
            configuration |= 1 << 7;
            configuration |= 1 << 5;
            configuration |= 1 << 4;

            _i2cDevice.Write(new byte[] { DS2482_100.FunctionCommand.WRITE_CONFIGURATION, configuration });
        }

        // TEST BUILD
        static byte[] dscrc_table = (new [] {
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
        byte docrc8(byte value)
        {
            // See Application Note 27

            // TEST BUILD
            crc8 = dscrc_table[crc8 ^ value];
            return crc8;
        }

        public void Dispose()
        {
            _i2cDevice.Dispose();
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
