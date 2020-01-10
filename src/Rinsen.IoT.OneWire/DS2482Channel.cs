using Rinsen.IoT.OneWire.Abstractions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rinsen.IoT.OneWire
{
    //
    // This implementation is based on Maxim sample implementation and has parts from ported C code
    // source: http://www.maximintegrated.com/en/app-notes/index.mvp/id/187
    //
    public class DS2482Channel : OneWireMaster
    {
        private readonly DS2482 _ds2482Device;
        private readonly OneWireChannel _channel;

        // global search state
        //public byte[] DeviceAddress { get; private set; } = new byte[8];
        

        public DS2482Channel(DS2482 ds2482, OneWireChannel channel)
        {
            _ds2482Device = ds2482;
            _channel = channel;
        }

        public override IEnumerator<byte[]> GetEnumerator()
        {
            return new DS2482ChannelEnumerator(this);
        }

        public List<IOneWireDevice> GetConnectedOneWireDevices(Dictionary<byte, Type> oneWireDeviceTypes)
        {
            var oneWireDevices = new List<IOneWireDevice>();
            
            foreach (var deviceAddress in this)
            {
                oneWireDevices.Add(AddOneWireDevice(oneWireDeviceTypes, deviceAddress));
            }

            //if (deviceDetected)
            //{
            //    var result = true;
            //    do
            //    {
            //        if (first)
            //        {
            //            first = false;
            //            result = OneWireFirst();
            //        }
            //        else
            //        {
            //            result = OnoWireNext();
            //        }

            //        if (result)
            //        {
                        
            //        }

            //    } while (result);
            //}

            return oneWireDevices;
        }

        private IOneWireDevice AddOneWireDevice(Dictionary<byte, Type> oneWireDeviceTypes, byte[] deviceAddress)
        {
            IOneWireDevice device;
            if (oneWireDeviceTypes.Any(k => k.Key == deviceAddress[0]))
            {
                device = (IOneWireDevice)Activator.CreateInstance(oneWireDeviceTypes.First(k => k.Key == deviceAddress[0]).Value);
                
            }
            else
            {
                device = new UndefinedOneWireDevice();
            }

            device.Initialize(this, new byte[deviceAddress.Length]);

            Array.Copy(deviceAddress, device.OneWireAddress, deviceAddress.Length);
            return device;
        }

        //--------------------------------------------------------------------------
        // 1-Wire Functions to be implemented for a particular platform
        //--------------------------------------------------------------------------

        /// <summary>
        /// Reset the 1-Wire bus and return the presence of any device
        /// </summary>
        /// <returns>true : device present, false : no device present</returns>
        public override bool Reset()
        {
            // platform specific
            // TMEX API TEST BUILD
            //return (TMTouchReset(session_handle) == 1);

            if(!_ds2482Device.IsCorrectChannelSelected(_channel))
            {
                SelectDS2482Channel();
            }

            _ds2482Device.I2cDevice.Write(new byte[] { FunctionCommand.ONEWIRE_RESET });

            var status = ReadStatus();

            if (status.GetBit(StatusBit.ShortDetected))
            {
                throw new InvalidOperationException("One Wire short detected");
            }

            return status.GetBit(StatusBit.PresencePulseDetected);
        }

        private void SelectDS2482Channel()
        {
            var statusBuffer = new byte[1];

            switch (_channel)
            {
                case OneWireChannel.Channel_IO0:
                    _ds2482Device.I2cDevice.WriteRead(new byte[] { FunctionCommand.CHANNEL_SELECT, ChannelSelect.Channel_IO0 }, statusBuffer);

                    if (statusBuffer[0] != ChannelSelectVerification.Channel_IO0)
                        throw new InvalidOperationException("Failed to set selected channel");

                    break;
                case OneWireChannel.Channel_IO1:
                    _ds2482Device.I2cDevice.WriteRead(new byte[] { FunctionCommand.CHANNEL_SELECT, ChannelSelect.Channel_IO1 }, statusBuffer);

                    if (statusBuffer[0] != ChannelSelectVerification.Channel_IO1)
                        throw new InvalidOperationException("Failed to set selected channel");

                    break;
                case OneWireChannel.Channel_IO2:
                    _ds2482Device.I2cDevice.WriteRead(new byte[] { FunctionCommand.CHANNEL_SELECT, ChannelSelect.Channel_IO2 }, statusBuffer);

                    if (statusBuffer[0] != ChannelSelectVerification.Channel_IO2)
                        throw new InvalidOperationException("Failed to set selected channel");

                    break;
                case OneWireChannel.Channel_IO3:
                    _ds2482Device.I2cDevice.WriteRead(new byte[] { FunctionCommand.CHANNEL_SELECT, ChannelSelect.Channel_IO3 }, statusBuffer);

                    if (statusBuffer[0] != ChannelSelectVerification.Channel_IO3)
                        throw new InvalidOperationException("Failed to set selected channel");

                    break;
                case OneWireChannel.Channel_IO4:
                    _ds2482Device.I2cDevice.WriteRead(new byte[] { FunctionCommand.CHANNEL_SELECT, ChannelSelect.Channel_IO4 }, statusBuffer);

                    if (statusBuffer[0] != ChannelSelectVerification.Channel_IO4)
                        throw new InvalidOperationException("Failed to set selected channel");

                    break;
                case OneWireChannel.Channel_IO5:
                    _ds2482Device.I2cDevice.WriteRead(new byte[] { FunctionCommand.CHANNEL_SELECT, ChannelSelect.Channel_IO5 }, statusBuffer);

                    if (statusBuffer[0] != ChannelSelectVerification.Channel_IO5)
                        throw new InvalidOperationException("Failed to set selected channel");

                    break;
                case OneWireChannel.Channel_IO6:
                    _ds2482Device.I2cDevice.WriteRead(new byte[] { FunctionCommand.CHANNEL_SELECT, ChannelSelect.Channel_IO6 }, statusBuffer);

                    if (statusBuffer[0] != ChannelSelectVerification.Channel_IO6)
                        throw new InvalidOperationException("Failed to set selected channel");

                    break;
                case OneWireChannel.Channel_IO7:
                    _ds2482Device.I2cDevice.WriteRead(new byte[] { FunctionCommand.CHANNEL_SELECT, ChannelSelect.Channel_IO7 }, statusBuffer);

                    if (statusBuffer[0] != ChannelSelectVerification.Channel_IO7)
                        throw new InvalidOperationException("Failed to set selected channel");

                    break;
                default:
                    throw new NotSupportedException();
            }

            _ds2482Device.SetSelectedChannel(_channel);
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
                _ds2482Device.I2cDevice.WriteRead(new byte[] { FunctionCommand.SET_READ_POINTER, RegisterSelection.STATUS }, statusBuffer);
            }
            else
            {
                _ds2482Device.I2cDevice.Read(statusBuffer);
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
                _ds2482Device.I2cDevice.Read(statusBuffer);
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
                _ds2482Device.I2cDevice.WriteRead(new byte[] { FunctionCommand.SET_READ_POINTER, RegisterSelection.STATUS }, status);
            } while (status[0].GetBit(StatusBit.OneWireBusy));
        }

        /// <summary>
        /// Send 8 bits of data to the 1-Wire bus
        /// </summary>
        /// <param name="byte_value">byte to send</param>
        public override void WriteByte(byte byte_value)
        {
            // platform specific

            // TMEX API TEST BUILD
            //TMTouchByte(session_handle, byte_value);

            _ds2482Device.I2cDevice.Write(new byte[] { FunctionCommand.ONEWIRE_WRITE_BYTE, byte_value });

            ReadStatus();
        }

        /// <summary>
        /// Send 1 bit of data to teh 1-Wire bus 
        /// </summary>
        /// <param name="bit_value"></param>
        public override void WriteBit(bool bit_value)
        {
            // platform specific

            // TMEX API TEST BUILD
            //TMTouchBit(session_handle, (short)bit_value);

            var byte_value = new byte();
            if (bit_value)
            {
                byte_value |= 1 << 7;
            }

            _ds2482Device.I2cDevice.Write(new byte[] { FunctionCommand.ONEWIRE_SINGLE_BIT, byte_value });


            ReadStatus();
        }

        /// <summary>
        /// Read 1 bit of data from the 1-Wire bus 
        /// </summary>
        /// <returns>true : bit read is 1, false : bit read is 0</returns>
        public override bool ReadBit()
        {
            // platform specific

            // TMEX API TEST BUILD
            //return (byte)TMTouchBit(session_handle, 0x01);

            var byte_value = new byte();

            byte_value |= 1 << 7;

            _ds2482Device.I2cDevice.Write(new[] { FunctionCommand.ONEWIRE_SINGLE_BIT, byte_value });

            var status = ReadStatus();

            return status.GetBit(StatusBit.SingleBitResult);
        }

        /// <summary>
        /// Read 1 bit of data from the 1-Wire bus 
        /// </summary>
        /// <returns></returns>
        public override byte ReadByte()
        {
            var buffer = new byte[1];
            _ds2482Device.I2cDevice.Write(new byte[] { FunctionCommand.ONEWIRE_READ_BYTE });
            ReadStatus();
            _ds2482Device.I2cDevice.WriteRead(new byte[] { FunctionCommand.SET_READ_POINTER, RegisterSelection.READ_DATA }, buffer);
            return buffer[0];
        }

        public void EnableStrongPullup()
        {
            var configuration = new byte();
            configuration |= 1 << 2;
            configuration |= 1 << 7;
            configuration |= 1 << 5;
            configuration |= 1 << 4;

            _ds2482Device.I2cDevice.Write(new byte[] { FunctionCommand.WRITE_CONFIGURATION, configuration });
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
        public static byte CalculateGlobalCrc8(byte value, byte crc8)
        {
            // See Application Note 27

            // TEST BUILD
            crc8 = dscrc_table[crc8 ^ value];
            return crc8;
        }

        public class FunctionCommand
        {
            public const byte DEVICE_RESET = 0xF0;
            public const byte SET_READ_POINTER = 0xE1;
            public const byte WRITE_CONFIGURATION = 0xD2;
            public const byte CHANNEL_SELECT = 0xC3;
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
            public const byte CHANNEL_SELECTION = 0xD2;
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

        public class ChannelSelect
        {
            public const byte Channel_IO0 = 0xF0;
            public const byte Channel_IO1 = 0xE1;
            public const byte Channel_IO2 = 0xD2;
            public const byte Channel_IO3 = 0xC3;
            public const byte Channel_IO4 = 0xB4;
            public const byte Channel_IO5 = 0xA5;
            public const byte Channel_IO6 = 0x96;
            public const byte Channel_IO7 = 0x87;
        }

        public class ChannelSelectVerification
        {
            public const byte Channel_IO0 = 0xB8;
            public const byte Channel_IO1 = 0xB1;
            public const byte Channel_IO2 = 0xAA;
            public const byte Channel_IO3 = 0xA3;
            public const byte Channel_IO4 = 0x9C;
            public const byte Channel_IO5 = 0x95;
            public const byte Channel_IO6 = 0x8E;
            public const byte Channel_IO7 = 0x87;
        }

        public enum OneWireChannel : byte
        {
            Channel_IO0 = 0xF0,
            Channel_IO1 = 0xE1,
            Channel_IO2 = 0xD2,
            Channel_IO3 = 0xC3,
            Channel_IO4 = 0xB4,
            Channel_IO5 = 0xA5,
            Channel_IO6 = 0x96,
            Channel_IO7 = 0x87
        }
    }
}