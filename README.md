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

Headed apps
-----------
Headed apps do not currently support disposing the OneWireDeviceHandler. The instance MUST be reused between measurements.

I2C Address
-----------

DS2482-100 supports up to 4 devices on the same bus, the bus control flags are exposed via OneWireDeviceHandler ctor. True/False is the same as high/low on the AD0 and AD1 pins on the device.

If the address is wrong or the device is connected in a bad way there will be a DS2482100DeviceNotFoundException thrown that will indicate that there is no connection to the DS2482-100 device but it does not know if it is related to addressing problems or physical connection problems, or i there is no device connected at all.

Built in One Wire Device Support
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
