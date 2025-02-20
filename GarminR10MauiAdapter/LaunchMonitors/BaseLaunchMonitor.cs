using GarminR10MauiAdapter.OpenConnect;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace GarminR10MauiAdapter.LaunchMonitors
{
    public abstract class BaseLaunchMonitor: IDisposable
    {
        #region Events

        /// <summary>
        /// Event handler for when the connection status changes.
        /// </summary>
        public Action<ConnectionStatus>? OnConnectionStatusChanged { get; set; }

        /// <summary>
        /// Event arguments for when the device is ready state changes.
        /// </summary>
        public Action<bool>? OnDeviceReady { get; set; }

        /// <summary>
        /// Event handler for when the device is ready state changes.
        /// </summary>
        public Action<LaunchMonitorShotData>? OnShot { get; set; }

        /// <summary>
        /// Event handler for when the battery life is updated. Value is percentage.
        /// </summary>
        public Action<int>? OnBatteryLife { get; set; }

        #endregion

        #region Protected Properties

        /// <summary>
        /// The current connection status.
        /// </summary>
        protected ConnectionStatus connectionStatus = ConnectionStatus.Disconnected;

        /// <summary>
        /// Launch Monitor settings.
        /// </summary>
        private LaunchMonitorSettings settings = new LaunchMonitorSettings();

        #endregion

        #region Constructor

        /// <summary>
        /// Direct connection to a Garmin R10 Launch Monitor device.
        /// </summary>
        public BaseLaunchMonitor()
        {
        }

        /// <summary>
        /// Direct connection to a Garmin R10 Launch Monitor device.
        /// </summary>
        /// <param name="settings"></param>
        public BaseLaunchMonitor(LaunchMonitorSettings settings)
        {
            if (settings != null)
            {
                this.settings = settings;
            }
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// The current connection status.
        /// </summary>
        public ConnectionStatus ConnectionStatus
        {
            get
            {
                return connectionStatus;
            }
        }

        /// <summary>
        /// The units the speed data is outputted in. Meters per second or Miles per hour.
        /// </summary>
        public SpeedUnit SpeedUnits
        {
            get
            {
                return settings.OutputUnits.Equals(Units.Metric) ? SpeedUnit.MPS : SpeedUnit.MPH;
            }
        }

        /// <summary>
        /// The units the distance data is outputted in. Meters or Yards.
        /// </summary>
        public DistanceUnit DistanceUnits
        {
            get
            {
                return settings.OutputUnits.Equals(Units.Metric) ? DistanceUnit.Meters : DistanceUnit.Yards;
            }
        }

        /// <summary>
        /// The units the outputted data is in. Metric or Imperial.
        /// </summary>
        public Units OutputUnits
        {
            get
            {
                return settings.OutputUnits;
            }
            set
            {
                settings.OutputUnits = value;
            }
        }

        /// <summary>
        /// OpenConnect client that the device is connected to.
        /// </summary>
        public OpenConnectClient? OpenConnectClient { get; protected set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Dispose the launch monitor.
        /// </summary>
        public virtual void Dispose()
        {
            if (OpenConnectClient != null)
            {
                OpenConnectClient.DisconnectAndStop();
                OpenConnectClient.Dispose();
                OpenConnectClient = null;
            }
        }

        /// <summary>
        /// Connects to a Garmin R10 Launch Monitor.
        /// </summary>
        /// <returns>A connection status.</returns>
        public abstract Task<ConnectionStatus> ConnectAsync();

        /// <summary>
        /// Disconnects from the device. Does not dispose this object.
        /// </summary>
        public virtual async Task DisconnectAsync()
        {
            OpenConnectClient?.DisconnectAndStop();
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Setup the OpenConnect client.
        /// </summary>
        protected void SetupOpenConnectClient()
        {
            if (settings.OpenConnectSettings != null)
            {
                //If OpenConnect client already exists, disconnect and dispose of it.
                if (OpenConnectClient != null)
                {
                    OpenConnectClient.DisconnectAndStop();
                    OpenConnectClient.Dispose();
                    OpenConnectClient = null;
                }

                OpenConnectClient = new OpenConnectClient(settings.OpenConnectSettings);
                OpenConnectClient.ConnectAsync();
            }
        }

        /// <summary>
        /// Event handler for when shot metrics are received.
        /// </summary>
        /// <param name="LaunchMonitorShotData">Shot data from launch monitor.</param>
        protected void InvokeOnShot(LaunchMonitorShotData shotData)
        {
            //See if there is player info to add to the shot data.
            if (OpenConnectClient != null && OpenConnectClient.LastKnownPlayerInfo != null)
            {
                if (shotData.Club == null && OpenConnectClient.LastKnownPlayerInfo.Club != null)
                {
                    shotData.Club = OpenConnectClient.LastKnownPlayerInfo.Club;
                }

                if (shotData.PlayerHanded == null && OpenConnectClient.LastKnownPlayerInfo.Handed != null)
                {
                    shotData.PlayerHanded = OpenConnectClient.LastKnownPlayerInfo.Handed.Value;
                }
            }

            // Invoke the OnShot event.
            if (OnShot != null)
            {
                //Send the shot data.
                Task.Run(() => OnShot.Invoke(shotData));
            }

            //Don't send practice swings to OpenConnect.
            if (OpenConnectClient != null && shotData.SwingType != SwingType.Practice)
            {
                var msg = shotData.ToOpenConnectApiMessage();
                if (msg.BallData != null)
                {
                    OpenConnectClient.SendAsync(msg);
                }
            }
        }

        /// <summary>
        /// Event handler for when the device is ready state changes.
        /// </summary>
        /// <param name="isReady">Inciates if device is ready to ready a shot or not.</param>
        protected void InvokeOnDeviceReady(bool isReady)
        {
            // Invoke the OnDeviceReady event.
            if (OnDeviceReady != null)
            {
                Task.Run(() => OnDeviceReady.Invoke(isReady));
            }

            OpenConnectClient?.SetDeviceReady(isReady);
        }

        /// <summary>
        /// Event handler for when the battery life is updated.
        /// </summary>
        /// <param name="lifePercentage">Batteyr life level in percentage.</param>
        protected void InvokeOnBatteryLife(int lifePercentage)
        {
            // Invoke the OnBatteryLife event.
            if (OnBatteryLife != null)
            {
                Task.Run(() => OnBatteryLife.Invoke(lifePercentage));
            }
        }

        #endregion

    }
}
