namespace iRacingButtonBox {
    using System;

    using ArduinoDriver;
    using ArduinoDriver.SerialProtocol;

    using ArduinoUploader.Hardware;

    using iRacingSdkWrapper;

    internal class Program {
        private static ArduinoDriver driver;

        private static void Main(string[] args) {
            var iracing = new SdkWrapper {
                              TelemetryUpdateFrequency = 20
                          };

            iracing.TelemetryUpdated += OnTelemetryUpdated;

            driver = new ArduinoDriver(ArduinoModel.UnoR3, "COM4", true);
            driver.Send(new PinModeRequest(2, PinMode.Output));

            Console.CursorVisible = false;

            iracing.Start();
        }

        private static void OnTelemetryUpdated(object sender, SdkWrapper.TelemetryUpdatedEventArgs e) { 
            var clutch = e.TelemetryInfo.Clutch.Value;
            var throttle = e.TelemetryInfo.Throttle.Value;
            var brake = e.TelemetryInfo.Brake.Value;

            Console.SetCursorPosition(0, 0);
            Console.WriteLine($"Clutch:\t\t{clutch:f5}");
            Console.WriteLine($"Throttle:\t{throttle:f5}");
            Console.WriteLine($"Brake:\t\t{brake:f5}");
        }
    }
}