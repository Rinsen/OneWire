using System;
using Windows.ApplicationModel.Background;
using Windows.System.Threading;
using Rinsen.IoT.OneWire;
using System.Diagnostics;
using System.Threading.Tasks;

// The Background Application template is documented at http://go.microsoft.com/fwlink/?LinkID=533884&clcid=0x409

namespace OneWire
{
    public sealed class StartupTask : IBackgroundTask
    {
        BackgroundTaskDeferral _deferral;
        private readonly IDS2482DeviceFactory _dS2482DeviceFactory = new DS2482DeviceFactory(); // This could be injected if using Generic Host for example

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            // 
            // TODO: Insert code to start one or more asynchronous methods 
            //

            // Keep this task running after this method has returned
            _deferral = taskInstance.GetDeferral();

            // Initial log
            Task.Run(LogTemperatures);
        }

        private async Task LogTemperatures()
        {
            using (var ds2482_800 = _dS2482DeviceFactory.CreateDS2482_800(false, false, false))
            using (var ds2482_100 = _dS2482DeviceFactory.CreateDS2482_100(true, true))
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
        }
    }
}
