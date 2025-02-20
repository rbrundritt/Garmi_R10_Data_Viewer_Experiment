namespace GarminR10MauiAdapter.OpenConnect
{
    /// <summary>
    /// Settings for the OpenConnect client
    /// </summary>
    public class OpenConnectSettings
    {
        /// <summary>
        /// IP Address of the OpenConnect client server.
        /// </summary>
        public string IpAddress { get; set; } = "127.0.0.1";

        /// <summary>
        /// Port of the OpenConnect client server.
        /// </summary>
        public string Port { get; set; } = "921";
    }
}
