namespace Rinsen.IoT.OneWire
{
    public class DS18S20 : DS18B20
    {
        
        public double GetExtendedTemperature()
        {
            byte[] scratchpad = GetTemperatureScratchpad();
            var temp_read = GetTemp_Read(scratchpad);

            return temp_read - 0.25 + ((scratchpad[Scratchpad.CountPerC] - scratchpad[Scratchpad.CountRemain]) / scratchpad[Scratchpad.CountPerC]);
        }

        protected override double GetTemp_Read(byte[] scratchpad)
        {
            double temp_read;
            var rawTemperature = scratchpad[Scratchpad.TemperatureLSB];
            var negativeSign = scratchpad[Scratchpad.TemperatureMSB] > 0;
            var decimalPart = rawTemperature.GetBit(0);
            var temperature = (int)rawTemperature;
            temperature &= ~(1 << 0);
            temperature = temperature >> 1;
            if (decimalPart)
            {
                temp_read = temperature + 0.5;
            }
            else
            {
                temp_read = temperature;
            }

            if (negativeSign)
            {
                temp_read = temp_read * -1;
            }

            return temp_read;
        }

        
        private class Scratchpad
        {
            public const int TemperatureLSB = 0;

            public const int TemperatureMSB = 1;

            public const int ThRegisterOrUserByte1 = 2;

            public const int TlRegisterOrUserByte2 = 3;

            public const int Reserved = 4;

            public const int Reserved2 = 5;

            public const int CountRemain = 6;

            public const int CountPerC = 7;

            public const int CRC = 8;

        }
    }
}
