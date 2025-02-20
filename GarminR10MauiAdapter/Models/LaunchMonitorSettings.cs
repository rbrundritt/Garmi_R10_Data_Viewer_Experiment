using GarminR10MauiAdapter.OpenConnect;

namespace GarminR10MauiAdapter
{
    public class LaunchMonitorSettings
    {
        /// <summary>
        /// Altitude of where the device is located.
        /// </summary>
        public float Altitude { get; set; } = 0;

        /// <summary>
        /// Units that the altitude is in.
        /// </summary>
        public DistanceUnit AltitudeUnits { get; set; } = DistanceUnit.Feet;

        /// <summary>
        /// Humidity level. Percent (0 - 1)
        /// </summary>
        public float Humidity { get; set; } = 0.5f;

        /// <summary>
        /// Air temperature where device is located.
        /// </summary>
        public float Temperature { get; set; } = 60;

        /// <summary>
        /// Units that the temperature is in.
        /// </summary>
        public TemperatureUnit TemperatureUnits { get; set; } = TemperatureUnit.Fahrenheit;

        /// <summary>
        /// Air density in kg/m^3
        /// </summary>
        public float AirDensity { get; set; } = 1.225f;

        /// <summary>
        /// The units of measurement of the output data. 
        /// If metric, the output will be in meters and meters per second.
        /// If imperial, the output will be in yards and miles per hour.
        /// </summary>
        public Units OutputUnits { get; set; } = Units.Imperial;

        /// <summary>
        /// OpenConnect client settings. Only specify this if you want to stream data to OpenConnect.
        /// </summary>
        public OpenConnectSettings? OpenConnectSettings = null;
    }
}
