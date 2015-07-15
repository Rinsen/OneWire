using System;
using Windows.ApplicationModel.Background;
using Rinsen.IoT.OneWire;
using Windows.System.Threading;

namespace OneWire
{
    public sealed class StartupTask : IBackgroundTask
    {
        BackgroundTaskDeferral _defferal;
        private ThreadPoolTimer _timer;

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            _defferal = taskInstance.GetDeferral();

            LogTemperatures(null);

            // Log every 5 minutes
            _timer = ThreadPoolTimer.CreatePeriodicTimer(LogTemperatures, TimeSpan.FromMinutes(5));
        }

        private void LogTemperatures(ThreadPoolTimer timer)
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
