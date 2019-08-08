One Wire in .NET C# for Raspberry Pi
=========================
via DS2482-100 or DS2482-800 for Windows IoT on Raspberry Pi

A new Beta version is released called 3.0-beta.

The single key feature in this release is a move away from UWP that is starting to feel quite dead now and to Net Standard instead. 
This should in theory give this project the possibility to also leave Windows IoT Core that feels quite dead now days also and instead run on .NET Core in Linux on the Raspberry PI while still supporting Windows IoT Core also. 
But I have not tested this possibility in any way.

This is made possible by the awesome work done by the ".NET Core IoT Libraries" project https://github.com/dotnet/iot/

The previous 2.0 release introduced all of these changes.

The key features of this release is
- Support for DS2482-800 chip
- a new DS2482DeviceFactory for creating DS2482 connections instead of the old OneWireDeviceHandler() that was hiding to much of what was going on.
- some minor tweaks, bug fixes and performance enhancements

Introduction
------------

A powerful library for a simple and easy to use API when communicating with One Wire devices via I2C on Raspberry Pi.

    using (var ds2482_800 = await _dS2482DeviceFactory.CreateDS2482_800(false, false, false))
    using (var ds2482_100 = await _dS2482DeviceFactory.CreateDS2482_100(true, true))
    {
        while (true)
        {
            foreach (var device in ds2482_800.GetDevices<DS18S20>())
            {
                var result = device.GetTemperature();
                var extendedResult = device.GetExtendedTemperature();
                Debug.WriteLine($"DS2482-800, DS18S20 result {result}");
                // Insert code to log result in some way
            }

            foreach (var device in ds2482_800.GetDevices<DS18B20>())
            {
                var result = device.GetTemperature();
                Debug.WriteLine($"DS2482-800, DS18B20 result {result}");

                // Insert code to log result in some way
            }

            foreach (var device in ds2482_100.GetDevices<DS18S20>())
            {
                var result = device.GetTemperature();
                var extendedResult = device.GetExtendedTemperature();
                Debug.WriteLine($"DS2482_100, DS18S20 result {result}");

                // Insert code to log result in some way
            }

            foreach (var device in ds2482_100.GetDevices<DS18B20>())
            {
                var result = device.GetTemperature();
                Debug.WriteLine($"DS2482-100, DS18B20 result {result}");

                // Insert code to log result in some way
            }

            await Task.Delay(5000);
        }
    }

And thats all you need to get started with measuring temperatures with a DS18B20 from .NET and C# on Raspberry Pi.

Headed apps
-----------
Headed apps do not currently support disposing the OneWireDeviceHandler. The instance MUST be reused between measurements.

I2C Address
-----------

Multiple DS2482-100 and DS2482-800 are supported at the same time on the same bus, the bus control flags are exposed via IDS2482DeviceFactory CreateDS2482_100(bool ad0, bool ad1) and CreateDS2482_800(bool ad0, bool ad1, bool ad2). 
True/False is the same as high/low on the AD0, AD1 and AD2 pins on the devices.

If the address is wrong or the device is connected in a bad way there will be a DS2482100DeviceNotFoundException thrown that will indicate that there is no connection to the DS2482 device but it does not know if it is related to addressing problems or physical connection problems, or i there is no device connected at all.

Built in One Wire Device Support
---------------------------------
##Today:
1. DS18B20
2. DS18S20

##Extend with your own device

    oneWireDeviceHandler.AddDeviceType<MyDeviceType>(OneWireFamilyCode);

Add the type and it´s one wire family code to the device handler, if there is no matching family code when new devices is discovered the will be created as a UnknownOneWireDevice.

The provided type also has to implement the IOneWireDevice interface.

    public interface IOneWireDevice
    {
        DS2482Channel DS2482Channel { get; }

        byte[] OneWireAddress { get; }

        void Initialize(DS2482Channel ds2482, byte[] oneWireAddress);
    }

For more information on how this works check the DS18B20 implementation.
