using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.Spi;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace TestApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private const string SPI_CONTROLLER_NAME = "SPI0";  /* For Raspberry Pi 2, use SPI0                            */
        private const Int32 SPI_CHIP_SELECT_LINE = 0;      /* Line 0 maps to physical pin number 24 on the Rpi2        */

        /*Uncomment if you are using mcp3208/3008 which is 12 bits output */

        byte[] readBuffer = new byte[3]; /*this is defined to hold the output data*/
        byte[] writeBuffer = new byte[3] { 0x06, 0x00, 0x00 };//00000110 00; // It is SPI port serial input pin, and is used to load channel configuration data into the device  

        /*Uncomment if you are using mcp3002*/
        //byte[] readBuffer = new byte[3]; /*this is defined to hold the output data*/  
        //byte[] writeBuffer = new byte[3] { 0x68, 0x00, 0x00 };//01101000 00; /* It is SPI port serial input pin, and is used to load channel configuration data into the device*/  

        private SpiDevice SpiDisplay;

        public static AppViewModel ViewModel;

        private DispatcherTimer timer;

        public MainPage()
        {
            this.InitializeComponent();
            MainPage.ViewModel = this.Resources["ViewModel"] as AppViewModel;
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            await InitSPI();
            this.timer = new DispatcherTimer();
            this.timer.Interval = TimeSpan.FromMilliseconds(1000);
            this.timer.Tick += Timer_Tick;
            this.timer.Start();
            base.OnNavigatedTo(e);
        }

        private void Timer_Tick(object sender, object e)
        {
            SpiDisplay.TransferFullDuplex(writeBuffer, readBuffer);
            ViewModel.BinaryString = $"{ToBinaryString(readBuffer[0])} {ToBinaryString(readBuffer[1])} {ToBinaryString(readBuffer[2])}";

            // 10100101
            // 00000111
            // &&&&&&&&
            // 00000101

            int result = readBuffer[1] & 0x07;
            result <<= 8;
            result += readBuffer[2];
            result >>= 1;

            ViewModel.AdcReading = result;

            var temp = CalculateTemperature(result);

            // temp -= 60; // pure fudge

            ViewModel.Temp = temp;
            Debug.WriteLine($"Reading : {result} // Temp : {temp.ToString()}");
        }

        private async Task InitSPI()
        {
            try
            {
                var settings = new SpiConnectionSettings(SPI_CHIP_SELECT_LINE);
                settings.ClockFrequency = 5000000;// 10000000;  
                settings.Mode = SpiMode.Mode0; //Mode3;  


                string spiAqs = SpiDevice.GetDeviceSelector(SPI_CONTROLLER_NAME);
                var deviceInfo = await DeviceInformation.FindAllAsync(spiAqs);
                SpiDisplay = await SpiDevice.FromIdAsync(deviceInfo[0].Id, settings);
            }

            /* If initialization fails, display the exception and stop running */
            catch (Exception ex)
            {
                throw new Exception("SPI Initialization Failed", ex);
            }
        }

        private double CalculateTemperature(int adcValue)
        {
            double volts = (adcValue * 3.3) / 1024; // calculate the voltage
            double ohms = ((1 / volts) * 3300) - 1000; // calculate the ohms of the thermististor

            double lnohm = Math.Log(ohms);

            double a = 0.000570569668444d;
            double b = 0.000239344111326d;
            double c = 0.000000047282773d;

            // Steinhart Hart Equation
            // T = 1/(a + b[ln(ohm)] + c[ln(ohm)]^3)

            double t1 = (b * lnohm); // b[ln(ohm)]

            double lnCubed = Math.Pow(lnohm, 3); // c[ln(ohm)]

            double t2 = c * lnCubed; // # c[ln(ohm)]^3

            double tempKelvin = 1 / (a + t1 + t2); // calcualte temperature

            double tempCelsius = tempKelvin - 273.15; // K to C

            var tempF = tempCelsius * 9 / 5 + 32;

            return tempF;
        }

        public string ToBinaryString(byte b)
        {
            return Convert.ToString(b, 2).PadLeft(8, '0');
        }
    }
}
