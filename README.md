One Wire in .NET C# for Raspberry Pi
=========================
via DS2482-100 for Windows IoT on Raspberry Pi

Introduction
------------

A powerful library for a simple and easy to use API when communicating with One Wire devices via I2C on Raspberry Pi.

    using (var oneWireDeviceHandler = new OneWireDeviceHandler())
    {
        foreach (var device in oneWireDeviceHandler.GetDevices<DS18B20>())
        {
            var result = device.GetTemperature();
            
            // Insert code to log result in some way
        }
    }

And thats all you need to get started with measuring temperatures with a DS18B20 from .NET and C# on Raspberry Pi.

Build in One Wire Device Support
--------------
##Today:
1. DS18B20
2. DS18S20

##Extend with your own device

    oneWireDeviceHandler.AddDeviceType<MyDeviceType>(OneWireFamilyCode);

Add the type and it´s one wire family code to the device handler, if there is no matching family code when new devices is discovered the will be created as a UnknownOneWireDevice.

The provided type also has to implement the IOneWireDevice interface.

    public interface IOneWireDevice
    {
        DS2482_100 DS2482_100 { get; set; }
        
        byte[] OneWireAddress { get; set; }
        
        void Initialize();
    }

For more information on how this works check the DS18B20 implementation.
