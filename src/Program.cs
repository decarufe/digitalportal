using System;
using System.Diagnostics;
using System.Net;
using System.Threading;

using GHIElectronics.TinyCLR.UI;
using GHIElectronics.TinyCLR.Native;
using GHIElectronics.TinyCLR.Devices.Display;
using GHIElectronics.TinyCLR.Drivers.FocalTech.FT5xx6;

// To run this application in production please register for your own free
// API key from http://home.openweathermap.org. You can update this key 
// from the UX => Settings => Weather Settings => App Id or via source code in the "flashobject.cs" file.

namespace Bytewizer.TinyCLR.DigitalPortal
{
    public class Program : Application
    {
        public static Program MainProgram;

        private static int timerTick = 0;

        public Program(DisplayController display)
            : base(display) { }

        static void Main()
        {
            if (!Memory.IsExtendedHeap())
            {
                Memory.ExtendHeap();
                Power.Reset();
            }

            Timer timer = new Timer(TimerTick, null, 3600000, 3600000); // 1 hour

            ClockProvider.Initialize();
            SettingsProvider.Initialize();
            
            //SettingsProvider.Flash.Ssid = "ssid";
            //SettingsProvider.Flash.Password = "password";
            //SettingsProvider.Flash.NetworkEnabled = true;

            BuzzerProvider.Initialize();
            DisplayProvider.Initialize();
            TouchProvider.Initialize();
            NetworkProvider.Initialize();
            WeatherProvider.Initialize();

            TouchProvider.Controller.TouchDown += Touch_TouchDown;
            TouchProvider.Controller.TouchUp += Touch_TouchUp;

            if (SettingsProvider.Flash.NetworkEnabled)
            {
                NetworkProvider.EnableWifi();
            }

            MainProgram = new Program(DisplayProvider.Controller);

            var mainWindow = new MainWindow(DisplayProvider.Width, DisplayProvider.Height);

            var clockPage = new ClockPage(DisplayProvider.Width, DisplayProvider.Height);
            mainWindow.Register(clockPage);

            var weatherPage = new WeatherPage(DisplayProvider.Width, DisplayProvider.Height);
            mainWindow.Register(weatherPage);

            var alarmPage = new AlarmPage(DisplayProvider.Width, DisplayProvider.Height);
            mainWindow.Register(alarmPage);

            var optionsPage = new OptionsPage(DisplayProvider.Width, DisplayProvider.Height);
            mainWindow.Register(optionsPage);

            var notAvailablePage = new NotAvailablePage(DisplayProvider.Width, DisplayProvider.Height);
            mainWindow.Register(notAvailablePage);

            var wifiPage = new WifiPage(DisplayProvider.Width, DisplayProvider.Height);
            mainWindow.Register(wifiPage);

            var themePage = new ThemePage(DisplayProvider.Width, DisplayProvider.Height);
            mainWindow.Register(themePage);

            var weatherSettingsPage = new OpenWeatherPage(DisplayProvider.Width, DisplayProvider.Height);
            mainWindow.Register(weatherSettingsPage);

            var AppearancePage = new AppearancePage(DisplayProvider.Width, DisplayProvider.Height);
            mainWindow.Register(AppearancePage);

            mainWindow.Activate(SettingsProvider.Flash.DefaultPage);

            MainProgram.Run(mainWindow);

            Timer keepAlive = new Timer(KeepAlive, null, TimeSpan.FromSeconds(20), TimeSpan.FromSeconds(20)); // 20 seconds
        }

        private static void TimerTick(object sender)
        {
            timerTick++;

            if (NetworkProvider.IsConnected)
            {
                if (timerTick >= 24)
                {
                    NetworkProvider.ConnectNetworkTime();
                    timerTick = 0;
                }
            }
        }

        private static void KeepAlive(object sender)
        {
            if (NetworkProvider.IsConnected)
            {
                var url = "http://www.bing.com/robots.txt";

                int read = 0, total = 0;
                byte[] result = new byte[512];

                try
                {
                    using (var req = WebRequest.Create(url) as HttpWebRequest)
                    {
                        req.KeepAlive = false;
                        req.ReadWriteTimeout = 2000;

                        using (var res = req.GetResponse() as HttpWebResponse)
                        {
                            using (var stream = res.GetResponseStream())
                            {
                                do
                                {
                                    read = stream.Read(result, 0, result.Length);
                                    total += read;

                                    Debug.WriteLine("read : " + read);
                                    Debug.WriteLine("total : " + total);

                                    String page = "";

                                    page = new String(System.Text.Encoding.UTF8.GetChars
                                        (result, 0, read));

                                    Debug.WriteLine("Response : " + page);
                                }

                                while (read != 0);
                            }
                        }
                    }
                }
                catch(Exception e)
                {
                    Debug.WriteLine(e.ToString());
                }

                if (total == 0)
                {
                    NetworkProvider.DisableWifi();
                    Thread.Sleep(1000);
                    NetworkProvider.EnableWifi();
                }
            }
        }

        private static void Touch_TouchUp(FT5xx6Controller sender, TouchEventArgs e) =>
            MainProgram.InputProvider.RaiseTouch(e.X, e.Y, GHIElectronics.TinyCLR.UI.Input.TouchMessages.Up, DateTime.Now);

        private static void Touch_TouchDown(FT5xx6Controller sender, TouchEventArgs e) =>
            MainProgram.InputProvider.RaiseTouch(e.X, e.Y, GHIElectronics.TinyCLR.UI.Input.TouchMessages.Down, DateTime.Now);
    }
}