namespace iRacingButtonBox {
    using System;
    using System.Diagnostics;

    using ArduinoDriver;
    using ArduinoDriver.SerialProtocol;

    using ArduinoUploader.Hardware;

    using iRacingSdkWrapper;

    internal static class Program {
        private static ArduinoDriver driver;
        private static TelemetryInfo Telemetry;

        private static void Main(string[] args) {
            var iracing = new SdkWrapper {
                              TelemetryUpdateFrequency = 15
                          };

            iracing.TelemetryUpdated += OnTelemetryUpdated;

            driver = new ArduinoDriver(ArduinoModel.UnoR3, "COM4", true);

            Console.CursorVisible = false;

            const byte DataPin = 12;
            const byte ClockPin = 11;
            const byte LatchPin = 10;

            driver.Send(new PinModeRequest(DataPin, PinMode.Output));
            driver.Send(new PinModeRequest(LatchPin, PinMode.Output));
            driver.Send(new PinModeRequest(ClockPin, PinMode.Output));


            driver.Send(new DigitalWriteRequest(LatchPin, DigitalValue.Low));
            var response = driver.Send(new ShiftOutRequest(DataPin, ClockPin, BitOrder.LSBFIRST, 115));
            driver.Send(new DigitalWriteRequest(LatchPin, DigitalValue.High));

            Console.WriteLine($"{response.Value} = {Convert.ToString(response.Value, 2).PadLeft(8, '0')}");

            iracing.Start();
        }

        private static void OnTelemetryUpdated(object sender, SdkWrapper.TelemetryUpdatedEventArgs e) {
            var stopwatch = Stopwatch.StartNew();

            const int IndicatorHalfWidth = 4;
            var shftInd = e.TelemetryInfo.ShiftIndicatorPct.Value;
            shftInd = shftInd.Remap(0, 1, 0, IndicatorHalfWidth);

            Console.SetCursorPosition(0, 0);

            Console.Write("Indicator:\t\t");

            byte indicator = 0;
            for (var i = 0; i < IndicatorHalfWidth*2; i++) {
                indicator = (byte)(indicator << 1);
                if (i < shftInd.RoundToInt() || i >= (IndicatorHalfWidth*2) - shftInd.RoundToInt()) {
                    indicator |= 1;
                }
            }

            Console.WriteLine(Convert.ToString(indicator, 2).PadLeft(8, '0'));

            driver.Send(new DigitalWriteRequest(10, DigitalValue.Low));
            driver.Send(new ShiftOutRequest(12, 11, BitOrder.MSBFIRST, indicator));
            driver.Send(new DigitalWriteRequest(10, DigitalValue.High));

            stopwatch.Stop();

            Console.WriteLine($"Bitshift duration:\t{stopwatch.ElapsedMilliseconds} ms");
        }

        public static float Remap(this float value, float from1, float to1, float from2, float to2) {
            return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
        }

        public static int RoundToInt(this float value) {
            return (int)Math.Round(value);
        }
    }
}