namespace GarminR10MauiAdapter
{
    /// <summary>
    /// Information about the device.
    /// </summary>
    public class DeviceInfo
    {
        /// <summary>
        /// The device id.
        /// </summary>
        public Guid? DeviceId { get; set; }

        /// <summary>
        /// The device name.
        /// </summary>
        public string? DeviceName { get; set; }

        /// <summary>
        /// The device model.
        /// </summary>
        public string? Model { get; set; }

        /// <summary>
        /// The device firmware.
        /// </summary>
        public string? Firmware { get; set; }

        /// <summary>
        /// The device battery.
        /// </summary>
        public int Battery { get; set; }

        /// <summary>
        /// The device serial number.
        /// </summary>
        public string? SerialNumber { get; set; }
    }
}
