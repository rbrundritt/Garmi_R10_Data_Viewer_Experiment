namespace GarminR10MauiAdapter
{
    /// <summary>
    /// Connection status states of device.
    /// </summary>
    public enum ConnectionStatus
    {
        /// <summary>
        /// Device is disconnected.
        /// </summary>
        Disconnected = 0,

        /// <summary>
        /// Connecting to device.
        /// </summary>
        Connecting = 1,

        /// <summary>
        /// Device is connected.
        /// </summary>
        Connected = 2,

        /// <summary>
        /// Failed to connect to device. Device is not found.
        /// </summary>
        Device_Not_Found = 3,

        /// <summary>
        /// Failed to connect to device. Exceeded maximum retries.
        /// </summary>
        Max_Retries_Exceeded = 4,

        /// <summary>
        /// Looking for device.
        /// </summary>
        Looking_For_Device = 5,

        /// <summary>
        /// Disconnecting from device.
        /// </summary>
        Disconnecting = 6
    }
}
