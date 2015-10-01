using System;
using Windows.ApplicationModel.Background;
using Windows.System.Threading;
using Rinsen.IoT.OneWire;

// The Background Application template is documented at http://go.microsoft.com/fwlink/?LinkID=533884&clcid=0x409

namespace OneWire
{
    public sealed class StartupTask : IBackgroundTask
    {
        ThreadPoolTimer _timer;

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            // 
            // TODO: Insert code to start one or more asynchronous methods 
            //

            // Initial log
            LogTemperatures(null);

            // Then log every 5 minutes
            _timer = ThreadPoolTimer.CreatePeriodicTimer(LogTemperatures, TimeSpan.FromMinutes(5));
        }

        void LogTemperatures(ThreadPoolTimer timer)
        {
            try
            {
                using (var oneWireDeviceHandler = new OneWireDeviceHandler())
                {
                    foreach (var device in oneWireDeviceHandler.GetDevices<DS18S20>())
                    {
                        var result = device.GetTemperature();
                        var extendedResult = device.GetExtendedTemperature();

                        // Insert code to log result in some way
                    }

                    foreach (var device in oneWireDeviceHandler.OneWireDevices.GetDevices<DS18B20>())
                    {
                        var result = device.GetTemperature();

                        // Insert code to log result in some way
                    }
                }
            }
            catch (Exception e)
            {
                // Insert code to log all exceptions!
            }
        }
    }
}
