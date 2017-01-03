using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Windows.Devices.I2c;

namespace Rinsen.IoT.OneWire
{
    //
    // This implementation is based on Maxim sample implementation and has parts from ported C code
    // source: http://www.maximintegrated.com/en/app-notes/index.mvp/id/187
    //

    public class DS2482_100 : DS2482
    {
        public DS2482_100(I2cDevice i2cDevice)
            :base(i2cDevice)
        {
            ROM_NO = new byte[8];
        }

        public override List<IOneWireDevice> GetConnectedOneWireDevices(Dictionary<byte, Type> oneWireDeviceTypes)
        {
            var oneWireDevices = new List<IOneWireDevice>();
            var first = true;
            var deviceDetected = OneWireReset();

            if (deviceDetected)
            {
                var result = true;
                do
                {
                    if (first)
                    {
                        first = false;
                        result = OneWireFirst();
                    }
                    else
                    {
                        result = OnoWireNext();
                    }

                    if (result)
                    {
                        oneWireDevices.Add(AddOneWireDevice(oneWireDeviceTypes));
                    }

                } while (result);
            }

            return oneWireDevices;
        }

        private IOneWireDevice AddOneWireDevice(Dictionary<byte, Type> oneWireDeviceTypes)
        {
            if (oneWireDeviceTypes.Any(k => k.Key == ROM_NO[0]))
            {
                var device = (IOneWireDevice)Activator.CreateInstance(oneWireDeviceTypes.First(k => k.Key == ROM_NO[0]).Value);
                device.Initialize(this, new byte[ROM_NO.Length]);

                Array.Copy(ROM_NO, device.OneWireAddress, ROM_NO.Length);
                return device;
            }
            else
            {
                return new UndefinedOneWireDevice(this, ROM_NO);
            }
        }

        
        
        /// <summary>
        /// Find the 'first' devices on the 1-Wire bus
        /// </summary>
        /// <returns>true : device found, ROM number in ROM_NO buffer, false : no device present</returns>
        public bool OneWireFirst()
        {
            // reset the search state
            _lastDiscrepancy = 0;
            _lastDeviceFlag = false;
            _lastFamilyDiscrepancy = 0;

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
            _crc8 = 0;

            // if the last call was not the last one
            if (!_lastDeviceFlag)
            {
                // 1-Wire reset
                if (!OneWireReset())
                {
                    // reset the search
                    _lastDiscrepancy = 0;
                    _lastDeviceFlag = false;
                    _lastFamilyDiscrepancy = 0;
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
                            if (id_bit_number < _lastDiscrepancy)
                                search_direction = ((ROM_NO[rom_byte_number] & rom_byte_mask) > 0);
                            else
                                // if equal to last pick 1, if not then pick 0
                                search_direction = (id_bit_number == _lastDiscrepancy);

                            // if 0 was picked then record its position in LastZero
                            if (!search_direction)
                            {
                                last_zero = id_bit_number;

                                // check for Last discrepancy in family
                                if (last_zero < 9)
                                    _lastFamilyDiscrepancy = last_zero;
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
                if (!((id_bit_number < 65) || (_crc8 != 0)))
                {
                    // search successful so set LastDiscrepancy,LastDeviceFlag,search_result
                    _lastDiscrepancy = last_zero;

                    // check for last device
                    if (_lastDiscrepancy == 0)
                        _lastDeviceFlag = true;

                    search_result = true;
                }
            }

            // if no device found then reset counters so next 'search' will be like a first
            if (!search_result || ROM_NO[0] == 0)
            {
                _lastDiscrepancy = 0;
                _lastDeviceFlag = false;
                _lastFamilyDiscrepancy = 0;
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
            ld_backup = _lastDiscrepancy;
            ldf_backup = _lastDeviceFlag;
            lfd_backup = _lastFamilyDiscrepancy;

            // set search to find the same device
            _lastDiscrepancy = 64;
            _lastDeviceFlag = false;

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
            _lastDiscrepancy = ld_backup;
            _lastDeviceFlag = ldf_backup;
            _lastFamilyDiscrepancy = lfd_backup;

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
            _lastDiscrepancy = 64;
            _lastFamilyDiscrepancy = 0;
            _lastDeviceFlag = false;
        }

        /// <summary>
        /// Setup the search to skip the current device type on the next call to OWNext().
        /// </summary>
        public void OneWireFamilySkipSetup()
        {
            // set the Last discrepancy to last family discrepancy
            _lastDiscrepancy = _lastFamilyDiscrepancy;
            _lastFamilyDiscrepancy = 0;

            // check for end of list
            if (_lastDiscrepancy == 0)
                _lastDeviceFlag = true;
        }

        //--------------------------------------------------------------------------
        // 1-Wire Functions to be implemented for a particular platform
        //--------------------------------------------------------------------------

        /// <summary>
        /// Reset the 1-Wire bus and return the presence of any device
        /// </summary>
        /// <returns>true : device present, false : no device present</returns>
        public override bool OneWireReset()
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
        public override void OneWireWriteByte(byte byte_value)
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
        public override void OneWireWriteBit(bool bit_value)
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
        public override bool OneWireReadBit()
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
        public override byte OneWireReadByte()
        {
            var buffer = new byte[1];
            _i2cDevice.Write(new byte[] { FunctionCommand.ONEWIRE_READ_BYTE });
            ReadStatus();
            _i2cDevice.WriteRead(new byte[] { FunctionCommand.SET_READ_POINTER, RegisterSelection.READ_DATA }, buffer);
            return buffer[0];
        }

        public override void EnableStrongPullup()
        {
            var configuration = new byte();
            configuration |= 1 << 2;
            configuration |= 1 << 7;
            configuration |= 1 << 5;
            configuration |= 1 << 4;

            _i2cDevice.Write(new byte[] { FunctionCommand.WRITE_CONFIGURATION, configuration });
        }
        
    }
}
