using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.Gpio;
using Windows.Devices.Spi;
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

namespace TestApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        //int clockPinNumber = 18;
        //int slaveOutputPinNumber = 23;
        //int slaveInputPinNumber = 24;
        //int commandSlavePinNumber = 25;

        //GpioPin clockPin;
        //GpioPin slaveOutputPin;
        //GpioPin slaveInputPin;
        //GpioPin commandSlavePin;

        //GpioOpenStatus clockPinStatus = GpioOpenStatus.PinUnavailable;
        //GpioOpenStatus slaveOutputPinStatus = GpioOpenStatus.PinUnavailable;
        //GpioOpenStatus slaveInputPinStatus = GpioOpenStatus.PinUnavailable;
        //GpioOpenStatus commandSlavePinStatus = GpioOpenStatus.PinUnavailable;

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

        private void Timer_Tick(object sender, object e)
        {
            SpiDisplay.TransferFullDuplex(writeBuffer, readBuffer);
            ViewModel.BinaryString = $"{ToBinaryString(readBuffer[0])} {ToBinaryString(readBuffer[1])} {ToBinaryString(readBuffer[2])}";

            int result = readBuffer[1] & 0x07;
            result <<= 8;
            result += readBuffer[2];
            result >>= 1;

            ViewModel.AdcReading = result;

            var temp = CalculateTemperature(result);

            temp -= 60;

            ViewModel.Temp = temp;
            Debug.WriteLine($"Reading : {result} // Temp : {temp.ToString()}");
        }


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

            // a, b, & c values from http://www.thermistor.com/calculators.php
            // using curve R (-6.2%/C @ 25C) Mil Ratio X
            //double a = 0.002197222470870d;
            //double b = 0.000161097632222d;
            //double c = 0.000000125008328d;

            double a = 0.000570569668444d;
            double b = 0.000239344111326d;
            double c = 0.000000047282773d;

            // New values take from https://github.com/CapnBry/HeaterMeter/blob/87071885e0f6a9cca1aa6d26e1b6ebf3518c2313/openwrt/package/linkmeter/luasrc/view/linkmeter/conf.htm
            //double a = 6.6853001e-04;
            //double b = 2.2231022e-04;
            //double c = 9.9680632e-08;

            // Steinhart Hart Equation
            // T = 1/(a + b[ln(ohm)] + c[ln(ohm)]^3)

            double t1 = (b * lnohm); // b[ln(ohm)]

            double c2 = c * lnohm; // c[ln(ohm)]

            double t2 = Math.Pow(c2, 3); // # c[ln(ohm)]^3

            double temp = 1 / (a + t1 + t2); // calcualte temperature

            double tempc = temp - 273.15;// - 4; // K to C

            var tempf = tempc * 9 / 5 + 32;
            // the -4 is error correction for bad python math

            return tempf;
        }
        public string ToBinaryString(byte b)
        {
            return Convert.ToString(b, 2).PadLeft(8, '0');
        }

    }


}
