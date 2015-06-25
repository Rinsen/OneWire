namespace Rinsen.IoT.OneWire
{
    class UndefinedOneWireDevice : IOneWireDevice
    {
        public DS2482_100 DS2482_100 { get; set; }

        public byte[] OneWireAddress { get; set; }

        public void Initialize()
        {
        }
    }
}
