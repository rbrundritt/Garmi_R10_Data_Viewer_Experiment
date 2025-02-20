using GarminR10MauiAdapter.Internal;
using LaunchMonitor.Proto;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using System.Diagnostics;

namespace GarminR10MauiAdapter.LaunchMonitors
{
    /// <summary>
    /// Direct connection to a Garmin R10 Launch Monitor device.
    /// </summary>
    public class GarminR10 : BaseLaunchMonitor
    {
        #region Private Properties

        /// <summary>
        /// Flag to detect redundant calls to Dispose().
        /// </summary>
        private bool disposedValue = false;

        /// <summary>
        /// Device settings.
        /// </summary>
        private GerminR10Settings garminSettings = new GerminR10Settings();

        /// <summary>
        /// Wrapper around the launch monitor.
        /// </summary>
        private GarminR10Device? launchMonitor = null;

        #endregion

        #region Constructor

        /// <summary>
        /// Direct connection to a Garmin R10 Launch Monitor device.
        /// </summary>
        public GarminR10(): base()
        {
        }
        
        /// <summary>
        /// Direct connection to a Garmin R10 Launch Monitor device.
        /// </summary>
        /// <param name="settings"></param>
        public GarminR10(GerminR10Settings settings) : base(settings)
        {
            if (settings != null)
            {
                this.garminSettings = settings;
            }
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// How much to multiply the spin rate by. A workaround solution to adjust the spin rate due to known Garmin R10 limitations.
        /// Typically a number between 1 and 3.
        /// Tip: Adjust this value as needed when the user changes clubs.
        /// </summary>
        public float SpinRateMultiplier { get; set; } = 1.0f;

        /// <summary>
        /// How much to multiply the spin axis by. A workaround solution to adjust the spin axis due to known Garmin R10 limitations.
        /// Typically a number between 1 and 5.
        /// Tip: Adjust this value as needed when the user changes clubs.
        /// </summary>
        public float SpinAxisMultiper { get; set; } = 1.0f;

        #endregion

        #region Public Methods

        /// <summary>
        /// Connects to a Garmin R10 Launch Monitor.
        /// </summary>
        /// <returns>A connection status.</returns>
        public override Task<ConnectionStatus> ConnectAsync()
        {
            return Task.Run(async () =>
            {
                //If a connection is already in progress, do nothing.
                if (connectionStatus == ConnectionStatus.Connecting)
                {
                    return connectionStatus;
                }

                //If already connected, disconnect first.
                if (connectionStatus == ConnectionStatus.Connected)
                {
                    await DisconnectAsync();
                }

                UpdateConnectionStatus(ConnectionStatus.Looking_For_Device);

                //Get the device name from settings, or use default.
                string deviceName = garminSettings.DeviceName ?? "Approach R10";

                //Get a list of Bluetooth paired devices.

                var devices = BaseBluetoothDevice.Adapter.ConnectedDevices;

                IDevice? device = null;

                //Find the device with the matching name.
                foreach (var pairedDev in devices)
                {
                    if (pairedDev != null && pairedDev.Name == deviceName)
                    {
                        device = pairedDev;
                    }
                }

                //Try scanning for device.
                if(device == null)
                {
                    await BaseBluetoothDevice.StartScanning(new ScanFilterOptions()
                    {
                        DeviceNames = new string[] { deviceName }
                    });

                    devices = BaseBluetoothDevice.Adapter.DiscoveredDevices;

                    foreach (var pairedDev in devices)
                    {
                        if (pairedDev != null && pairedDev.Name == deviceName)
                        {
                            device = pairedDev;
                        }
                    }
                }

                if (device == null)
                {
                    //Device not found in list of paired devices.
                    //Device must be paired through computer/phone bluetooth settings before running.
                    //User should verify device name is correct.
                    UpdateConnectionStatus(ConnectionStatus.Device_Not_Found);
                    return connectionStatus;
                }

                UpdateConnectionStatus(ConnectionStatus.Connecting);

                //Attempt to connect to the device.
                int retryAttempts = 0;

                Debug.WriteLine($"Connecting to {device.Name}: {device.Id}");

                do
                {
                    var connectParameters = new ConnectParameters(true, true);
                    await BaseBluetoothDevice.Adapter.ConnectToDeviceAsync(device, connectParameters);

                    //If it was unable to connect, wait and try again.
                    if (device.State == DeviceState.Disconnected)
                    {
                        //If we have exceeded the max number of retries, give up.
                        if (retryAttempts >= garminSettings.MaxReconnectAttempts)
                        {
                            UpdateConnectionStatus(ConnectionStatus.Max_Retries_Exceeded);
                            return connectionStatus;
                        }

                        Debug.WriteLine($"Could not connect to bluetooth device. Waiting {garminSettings.ConnectionRetryInterval} seconds before trying again");
                        Thread.Sleep(TimeSpan.FromSeconds(garminSettings.ConnectionRetryInterval));

                        retryAttempts++;
                    }
                }
                while (device.State == DeviceState.Disconnected);

                Debug.WriteLine("Connected to Launch Monitor");

                SetupOpenConnectClient();

                //Setup the Launch Monitor.
                SetupLaunchMonitor(device);
                BaseBluetoothDevice.MonitorDeviceConnection(device, OnDeviceDisconnected);

                UpdateConnectionStatus(ConnectionStatus.Connected);
                return connectionStatus;
            });
        }

        /// <summary>
        /// Disconnects from the device. Does not dispose this object.
        /// </summary>
        public override async Task DisconnectAsync()
        {
            UpdateConnectionStatus(ConnectionStatus.Disconnecting);

            if (launchMonitor != null)
            {
                await BaseBluetoothDevice.Adapter.DisconnectDeviceAsync(launchMonitor.Device);
                launchMonitor.Dispose();
                launchMonitor = null;
            }

            await base.DisconnectAsync();

            UpdateConnectionStatus(ConnectionStatus.Disconnected);
        }

        /// <summary>
        /// Disposes this object.
        /// </summary>
        public override void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Gets the device info.
        /// </summary>
        /// <returns></returns>
        public DeviceInfo? GetDeviceInfo()
        {
            if (launchMonitor != null && launchMonitor.Device != null)
            {
                return new DeviceInfo()
                {
                    DeviceId = launchMonitor.Device.Id,
                    DeviceName = launchMonitor.Device.Name,
                    SerialNumber = launchMonitor.Serial,
                    Model = launchMonitor.Model,
                    Firmware = launchMonitor.Firmware,
                    Battery = launchMonitor.Battery
                };
            }

            return null;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Updates the connection status property and fires the event.
        /// </summary>
        /// <param name="status"></param>
        private void UpdateConnectionStatus(ConnectionStatus status)
        {
            connectionStatus = status;

            // Invoke the OnConnectionStatusChanged event.
            if (OnConnectionStatusChanged != null)
            {
                Task.Run(() => OnConnectionStatusChanged.Invoke(connectionStatus));
            }
        }

        /// <summary>
        /// Event handler for when the device disconnects.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void OnDeviceDisconnected()
        {
            UpdateConnectionStatus(ConnectionStatus.Disconnected);

            InvokeOnDeviceReady(false);

            Debug.WriteLine("Lost bluetooth connection");
        }

        /// <summary>
        /// Setups up the launch monitor.
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        private void SetupLaunchMonitor(IDevice device)
        {
            launchMonitor = new GarminR10Device(device);
            launchMonitor.AutoWake = garminSettings.AutoWake;
            launchMonitor.CalibrateTiltOnConnect = garminSettings.CalibrateTiltOnConnect;

            launchMonitor.ReadinessChanged += LaunchMonitor_ReadinessChanged;
            launchMonitor.ShotMetrics += LaunchMonitor_ShotMetricsRecieved;
            launchMonitor.BatteryLifeUpdated += LaunchMonitor_BatteryLifeUpdated;

            if (!launchMonitor.Setup())
            {
                Debug.WriteLine("Failed Device Setup");
                return;
            }

            var t = launchMonitor.ShotConfig(
                Utils.ConvertTemperature(garminSettings.Temperature, garminSettings.TemperatureUnits, TemperatureUnit.Fahrenheit), //TODO: Verify units.
                garminSettings.Humidity,
                Utils.ConvertDistance(garminSettings.Altitude, garminSettings.AltitudeUnits, DistanceUnit.Feet), //TODO: Verify units.
                garminSettings.AirDensity,
                Utils.ConvertDistance(garminSettings.TeeDistance, garminSettings.TeeDistanceUnits, DistanceUnit.Meters));

            Debug.WriteLine($"Device Setup Complete: \n\tModel: {launchMonitor.Model}\n\tFirmware: {launchMonitor.Firmware}\n\tBluetooth ID: {launchMonitor.Device.Id}\n\tBattery: {launchMonitor.Battery}%\n\tCurrent State: {launchMonitor.CurrentState}\n\tTilt: {launchMonitor.DeviceTilt}");

            //Send initial device states.
            InvokeOnBatteryLife(launchMonitor.Battery);
            InvokeOnDeviceReady(true);
        }

        /// <summary>
        /// Event handler for when the battery life is updated.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LaunchMonitor_BatteryLifeUpdated(object sender, BaseBluetoothDevice.BatteryEventArgs e)
        {
            InvokeOnBatteryLife(e.Battery);
        }

        /// <summary>
        /// Event handler for when the device is ready state changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LaunchMonitor_ReadinessChanged(object sender, GarminR10Device.ReadinessChangedEventArgs e)
        {
            // Invoke the OnDeviceReady event.
            InvokeOnDeviceReady(e.Ready);
        }

        /// <summary>
        /// Event handler for when shot metrics are received.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LaunchMonitor_ShotMetricsRecieved(object sender, GarminR10Device.MetricsEventArgs e)
        {
            var metrics = e.Metrics;

            if (metrics == null)
            {
                return;
            }

            var swingType = metrics.ShotType == Metrics.Types.ShotType.Normal ? SwingType.Normal : SwingType.Practice;

            if (swingType == SwingType.Practice && garminSettings.IgnorePracticeSwings)
            {
                return;
            }
           
            var data = new LaunchMonitorShotData()
            {
                ShotNumber = (int)metrics.ShotId,
                SwingType = swingType
            };

            //Capture ball data.
            var ballMetrics = metrics.BallMetrics;

            if (ballMetrics != null)
            {
                //Ignore shots with zero launch angle.
                if (garminSettings.IgnoreZeroLaunchAngleShots && ballMetrics.LaunchAngle == 0)
                {
                    return;
                }

                data.BallSpeed = Utils.ConvertSpeed(ballMetrics.BallSpeed, SpeedUnit.MPS, SpeedUnits); //Convert speed to desired units.
                data.HorizontalLaunchAngle = ballMetrics.LaunchDirection;
                data.VerticalLaunchAngle = ballMetrics.LaunchAngle;
                data.SpinAxis = ballMetrics.SpinAxis * -1 * SpinAxisMultiper;
                data.SpinRate = ballMetrics.TotalSpin * SpinRateMultiplier;

                //Capture spin calculation type.
                var spinCalculationType = SpinMethod.Estimated;

                if (ballMetrics.SpinCalculationType == BallMetrics.Types.SpinCalculationType.Measured ||
                    ballMetrics.SpinCalculationType == BallMetrics.Types.SpinCalculationType.BallFlight)
                {
                    spinCalculationType = SpinMethod.Actual;
                }

                data.SpinMethod = spinCalculationType;
            }

            //Capture club data.
            var clubMetrics = metrics.ClubMetrics;

            if (clubMetrics != null)
            {
                data.ClubSpeed = Utils.ConvertSpeed(clubMetrics.ClubHeadSpeed, SpeedUnit.MPS, SpeedUnits);
                data.AngleOfAttack = clubMetrics.AttackAngle;
                data.FaceToTarget = clubMetrics.ClubAngleFace;
                data.ClubPath = clubMetrics.ClubAnglePath;
            }

            //Capture swing data.
            var swingMetrics = metrics.SwingMetrics;

            if (swingMetrics != null)
            {
                int backswingDuration = (int)(swingMetrics.DownSwingStartTime - swingMetrics.BackSwingStartTime);
                int downswingDuration = (int)(swingMetrics.ImpactTime - swingMetrics.DownSwingStartTime);

                data.BackswingTime = new TimeSpan(0, 0, 0, backswingDuration); //TODO: double check this is seconds.
                data.DownswingTime = new TimeSpan(0, 0, 0, downswingDuration); //TODO: double check this is seconds.
            }

            // Invoke the OnShot event.
            InvokeOnShot(data);
        }

        /// <summary>
        /// Disposes of the object.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (launchMonitor != null)
                    {
                        launchMonitor.ReadinessChanged -= LaunchMonitor_ReadinessChanged;
                        launchMonitor.ShotMetrics -= LaunchMonitor_ShotMetricsRecieved;
                        launchMonitor.BatteryLifeUpdated -= LaunchMonitor_BatteryLifeUpdated;

                        launchMonitor.Dispose();
                    }

                    base.Dispose();
                }

                disposedValue = true;
            }
        }

        #endregion
    }
}
