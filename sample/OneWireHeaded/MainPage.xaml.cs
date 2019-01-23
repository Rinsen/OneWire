using Rinsen.IoT.OneWire;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace OneWireHeaded
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private readonly IDS2482DeviceFactory _dS2482DeviceFactory = new DS2482DeviceFactory();
        private readonly DS2482_100 _ds2482_100;

        public MainPage()
        {
            _ds2482_100 = _dS2482DeviceFactory.CreateDS2482_100(true, true).Result;
            this.InitializeComponent();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                foreach (var device in _ds2482_100.GetDevices<DS18B20>())
                {
                    var result = device.GetTemperature();
                    this.textBox.Text = result.ToString();
                }
            }
            catch (Exception exception)
            {
                this.textBox.Text = exception.Message;
            }
        }
    }
}
