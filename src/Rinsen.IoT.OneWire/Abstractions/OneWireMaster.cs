using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Rinsen.IoT.OneWire.Abstractions
{
    public abstract class OneWireMaster : IEnumerable<byte[]>, IDisposable
    {

        /// <summary>
        /// Reset the 1-Wire bus and return the presence of any device
        /// </summary>
        /// <returns>true : device present, false : no device present</returns>
        public abstract bool Reset();

        /// <summary>
        /// Send 8 bits of data to the 1-Wire bus
        /// </summary>
        /// <param name="byte_value">byte to send</param>
        public abstract void WriteByte(byte byte_value);

        /// <summary>
        /// Send 1 bit of data to the 1-Wire bus 
        /// </summary>
        /// <param name="bit_value"></param>
        public abstract void WriteBit(bool bit_value);

        /// <summary>
        /// Read 1 bit of data from the 1-Wire bus 
        /// </summary>
        /// <returns>true : bit read is 1, false : bit read is 0</returns>
        public abstract bool ReadBit();

        /// <summary>
        /// Read 1 bit of data from the 1-Wire bus 
        /// </summary>
        /// <returns></returns>
        public abstract byte ReadByte();

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            // Nothing to do in base class.
        }

        public abstract IEnumerator<byte[]> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
