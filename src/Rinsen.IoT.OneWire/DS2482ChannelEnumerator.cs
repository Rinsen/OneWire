using Rinsen.IoT.OneWire.Abstractions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Rinsen.IoT.OneWire
{
    public class DS2482ChannelEnumerator : IEnumerator<byte[]>
    {
        private readonly OneWireMaster _oneWireMaster;
        private readonly byte[] _deviceAddress = new byte[8];
        protected int _lastDiscrepancy;
        protected int _lastFamilyDiscrepancy;
        protected bool _lastDeviceFlag;
        protected byte _crc8;
        private bool _first = true;

        internal DS2482ChannelEnumerator(OneWireMaster oneWireMaster)
        {
            _oneWireMaster = oneWireMaster;
        }

        public byte[] Current { get; private set; } = new byte[8];

        object IEnumerator.Current => Current;

        public void Dispose()
        {
            // nothing to dispose
        }

        public bool MoveNext()
        {
            if (_first)
                Reset();

            if (OneWireSearch())
            {
                Array.Copy(_deviceAddress, Current, _deviceAddress.Length);

                return true;
            }

            return false;
        }

        public void Reset()
        {
            _lastDiscrepancy = 0;
            _lastDeviceFlag = false;
            _lastFamilyDiscrepancy = 0;
            _first = true;
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
                if (!_oneWireMaster.Reset())
                {
                    // reset the search
                    _lastDiscrepancy = 0;
                    _lastDeviceFlag = false;
                    _lastFamilyDiscrepancy = 0;
                    return false;
                }

                // issue the search command 
                _oneWireMaster.WriteByte(0xF0);

                // loop to do the search
                do
                {
                    // read a bit and its complement
                    id_bit = _oneWireMaster.ReadBit();
                    cmp_id_bit = _oneWireMaster.ReadBit();

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
                                search_direction = ((_deviceAddress[rom_byte_number] & rom_byte_mask) > 0);
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
                            _deviceAddress[rom_byte_number] |= rom_byte_mask;
                        else
                        {
                            var result = (byte)~rom_byte_mask;
                            _deviceAddress[rom_byte_number] &= result;
                        }


                        // serial number search direction write bit
                        _oneWireMaster.WriteBit(search_direction);

                        // increment the byte counter id_bit_number
                        // and shift the mask rom_byte_mask
                        id_bit_number++;
                        rom_byte_mask <<= 1;

                        // if the mask is 0 then go to new SerialNum byte rom_byte_number and reset mask
                        if (rom_byte_mask == 0)
                        {
                            DS2482Channel.CalculateGlobalCrc8(_deviceAddress[rom_byte_number], _crc8);  // accumulate the CRC
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
            if (!search_result || _deviceAddress[0] == 0)
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
                rom_backup[i] = _deviceAddress[i];
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
                    if (rom_backup[i] != _deviceAddress[i])
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
                _deviceAddress[i] = rom_backup[i];
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
            _deviceAddress[0] = family_code;
            for (i = 1; i < 8; i++)
                _deviceAddress[i] = 0;
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
    }
}
