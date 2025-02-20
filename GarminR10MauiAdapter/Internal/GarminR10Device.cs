using GarminR10MauiAdapter.Internal;
using Google.Protobuf;
using LaunchMonitor.Proto;
using Plugin.BLE.Abstractions.Contracts;
using System.Diagnostics;
using static LaunchMonitor.Proto.State.Types;
using static LaunchMonitor.Proto.SubscribeResponse.Types;
using static LaunchMonitor.Proto.WakeUpResponse.Types;

namespace GarminR10MauiAdapter
{
    internal class GarminR10Device : BaseBluetoothDevice
    {
        internal static Guid MEASUREMENT_SERVICE_UUID = Guid.Parse("6A4E3400-667B-11E3-949A-0800200C9A66");
        internal static Guid MEASUREMENT_CHARACTERISTIC_UUID = Guid.Parse("6A4E3401-667B-11E3-949A-0800200C9A66");
        internal static Guid CONTROL_POINT_CHARACTERISTIC_UUID = Guid.Parse("6A4E3402-667B-11E3-949A-0800200C9A66");
        internal static Guid STATUS_CHARACTERISTIC_UUID = Guid.Parse("6A4E3403-667B-11E3-949A-0800200C9A66");

        private HashSet<uint> ProcessedShotIDs = new HashSet<uint>();

        private StateType _currentState;
        internal StateType CurrentState
        {
            get { return _currentState; }
            private set
            {
                _currentState = value;
                Ready = value == StateType.Waiting;
            }
        }

        internal Tilt? DeviceTilt { get; private set; }

        private bool _ready = false;
        public bool Ready
        {
            get { return _ready; }
            private set
            {
                bool changed = _ready != value;
                _ready = value;
                if (changed)
                    ReadinessChanged?.Invoke(this, new ReadinessChangedEventArgs() { Ready = value });
            }
        }

        public bool AutoWake { get; set; } = true;
        public bool CalibrateTiltOnConnect { get; set; } = true;

        public event ReadinessChangedEventHandler? ReadinessChanged;
        public delegate void ReadinessChangedEventHandler(object sender, ReadinessChangedEventArgs e);
        public class ReadinessChangedEventArgs : EventArgs
        {
            public bool Ready { get; set; }
        }

        public event ErrorEventHandler? Error;
        public delegate void ErrorEventHandler(object sender, ErrorEventArgs e);
        public class ErrorEventArgs : EventArgs
        {
            public string? Message { get; set; }
            internal Error.Types.Severity Severity { get; set; }
        }

        public event MetricsEventHandler? ShotMetrics;
        public delegate void MetricsEventHandler(object sender, MetricsEventArgs e);
        public class MetricsEventArgs : EventArgs
        {
            internal Metrics? Metrics { get; set; }
        }

        public GarminR10Device(IDevice device) : base(device)
        {

        }

        public override bool Setup()
        {
            //Debug.WriteLine("Subscribing to measurement service");
            //var measService = Device.GetServiceAsync(MEASUREMENT_SERVICE_UUID).WaitAsync(TimeSpan.FromSeconds(5)).Result;
            //var measCharacteristic = GetCharacteristic(measService, MEASUREMENT_CHARACTERISTIC_UUID);

            //if (!measCharacteristic.StartUpdatesAsync().Wait(TimeSpan.FromSeconds(5)))
            //{
            //    Debug.WriteLine("Error subscribing to measurement characteristic");
            //}

            //// Bytes that come after each shot. No idea how to parse these
            //measCharacteristic.ValueUpdated += (o, e) => {
            //    var m = e;
            //}; //TODO: Investigate what to do with this

            //Debug.WriteLine("Subscribing to control service");
            //var controlPoint = GetCharacteristic(measService, CONTROL_POINT_CHARACTERISTIC_UUID);
            //if (!controlPoint.StartUpdatesAsync().Wait(TimeSpan.FromSeconds(5)))
            //{
            //    Debug.WriteLine("Error subscribing to the control characteristic");
            //}
            //// Response to waiting device through controlPointInterface. Unused for now
            //controlPoint.ValueUpdated += (o, e) => {
            //    var m = e;
            //}; //TODO: Investigate what to do with this

            //Debug.WriteLine("Subscribing to status service"); ;
            //var statusCharacteristic = GetCharacteristic(measService, STATUS_CHARACTERISTIC_UUID);
            //if (!statusCharacteristic.StartUpdatesAsync().Wait(TimeSpan.FromSeconds(5)))
            //{
            //    Debug.WriteLine("Error subscribing to the status characteristic");
            //}
            //statusCharacteristic.ValueUpdated += (o, e) =>
            //{
            //    bool isAwake = e.Characteristic.Value[1] == 0;
            //    bool isReady = e.Characteristic.Value[2] == 0;

            //    // the following is unused in favor of the status change notifications and wake control provided by the protobuf service
            //    // if (!isAwake)
            //    // {
            //    //   controlPoint.WriteValueWithResponseAsync(new byte[] { 0x00 }).Wait();
            //    // }
            //};

            bool baseSetupSuccess = base.Setup();
            if (!baseSetupSuccess)
            {
                Debug.WriteLine("Error during base device setup");
                return false;
            }

            WakeDevice();
            CurrentState = StatusRequest() ?? StateType.Error;
            DeviceTilt = GetDeviceTilt();
            SubscribeToAlerts().First();

            if (CalibrateTiltOnConnect)
            {
                StartTiltCalibration();
            }

            return true;
        }

        public override void HandleProtobufRequest(IMessage request)
        {
            if (request is WrapperProto WrapperProtoRequest)
            {
                AlertDetails notification = WrapperProtoRequest.Event.Notification.AlertNotification_;
                if (notification.State != null)
                {
                    CurrentState = notification.State.State_;
                    if (notification.State.State_ == StateType.Standby)
                    {
                        if (AutoWake)
                        {
                            Debug.WriteLine("Device asleep. Sending wakeup call");
                            WakeDevice();
                        }
                        else
                        {
                            Debug.WriteLine("Device asleep. Wake device using button (or enable autowake in settings)");
                        }
                    }
                }
                if (notification.Error != null && notification.Error.HasCode)
                {
                    Error?.Invoke(this, new ErrorEventArgs() { Message = $"{notification.Error.Code.ToString()} {notification.Error.DeviceTilt}", Severity = notification.Error.Severity });
                }
                if (notification.Metrics != null)
                {
                    //Check to see if duplicate shot was received. If so, ignore.
                    if (!ProcessedShotIDs.Contains(notification.Metrics.ShotId))
                    {
                        ProcessedShotIDs.Add(notification.Metrics.ShotId);
                        ShotMetrics?.Invoke(this, new MetricsEventArgs() { Metrics = notification.Metrics });
                    }
                }
                if (notification.TiltCalibration != null)
                {
                    DeviceTilt = GetDeviceTilt();
                }
            }
        }

        internal Tilt? GetDeviceTilt()
        {
            IMessage? resp = SendProtobufRequest(
              new WrapperProto() { Service = new LaunchMonitorService() { TiltRequest = new TiltRequest() } }
            );

            if (resp is WrapperProto WrapperProtoResponse)
            {
                return WrapperProtoResponse.Service.TiltResponse.Tilt;
            }

            return null;
        }

        internal ResponseStatus? WakeDevice()
        {
            IMessage? resp = SendProtobufRequest(
              new WrapperProto() { Service = new LaunchMonitorService() { WakeUpRequest = new WakeUpRequest() } }
            );

            if (resp is WrapperProto WrapperProtoResponse)
            {
                return WrapperProtoResponse.Service.WakeUpResponse.Status;
            }

            return null;
        }

        internal StateType? StatusRequest()
        {
            IMessage? resp = SendProtobufRequest(
              new WrapperProto() { Service = new LaunchMonitorService() { StatusRequest = new StatusRequest() } }
            );

            if (resp is WrapperProto WrapperProtoResponse)
            {
                return WrapperProtoResponse.Service.StatusResponse.State.State_;
            }

            return null;
        }

        internal List<AlertStatusMessage> SubscribeToAlerts()
        {
            IMessage? resp = SendProtobufRequest(
              new WrapperProto()
              {
                  Event = new EventSharing()
                  {
                      SubscribeRequest = new SubscribeRequest()
                      {
                          Alerts = { new List<AlertMessage>() { new AlertMessage() { Type = AlertNotification.Types.AlertType.LaunchMonitor } } }
                      }
                  }
              }
            );

            if (resp is WrapperProto WrapperProtoResponse)
            {
                return WrapperProtoResponse.Event.SubscribeRespose.AlertStatus.ToList();
            }

            return new List<AlertStatusMessage>();

        }

        /// <summary>
        /// Configures the conditions for the launch monitor.
        /// </summary>
        /// <param name="temperature">Air temperature in Celsius. Range -10C to 55C (14F to 131F)</param> //TODO: Might actually need to be in fahrenheit.
        /// <param name="humidity">Humidity. Range of 0 to 1.</param>
        /// <param name="altitude">Altitude of device in meters. Range 0 to 3,048m, 0 to 10,0000ft.</param>   //TODO: Might actually need to be in feet.
        /// <param name="airDensity">Air density (kg/m^3)</param>
        /// <param name="teeRange">Distance from launch monitor to tee in meters. Range 1.8 to 2.4m)</param>
        /// <returns></returns>
        public bool ShotConfig(float temperature, float humidity, float altitude, float airDensity, float teeRange)
        {
            IMessage? resp = SendProtobufRequest(new WrapperProto()
            {
                Service = new LaunchMonitorService()
                {
                    ShotConfigRequest = new ShotConfigRequest()
                    {
                        Temperature = temperature,
                        Humidity = humidity,
                        Altitude = altitude,
                        AirDensity = airDensity,
                        TeeRange = teeRange
                    }
                }
            });

            if (resp is WrapperProto WrapperProtoResponse)
            {
                return WrapperProtoResponse.Service.ShotConfigResponse.Success;
            }

            return false;
        }

        internal ResetTiltCalibrationResponse.Types.Status? ResetTiltCalibrartion(bool shouldReset = true)
        {
            IMessage? resp = SendProtobufRequest(
              new WrapperProto() { Service = new LaunchMonitorService() { ResetTiltCalRequest = new ResetTiltCalibrationRequest() { ShouldReset = shouldReset } } }
            );

            if (resp is WrapperProto WrapperProtoResponse)
            {
                return WrapperProtoResponse.Service.ResetTiltCalResponse.Status;
            }

            return null;
        }

        internal StartTiltCalibrationResponse.Types.CalibrationStatus? StartTiltCalibration(bool shouldReset = true)
        {
            IMessage? resp = SendProtobufRequest(
              new WrapperProto() { Service = new LaunchMonitorService() { StartTiltCalRequest = new StartTiltCalibrationRequest() } }
            );

            if (resp is WrapperProto WrapperProtoResponse)
            {
                return WrapperProtoResponse.Service.StartTiltCalResponse.Status;
            }

            return null;
        }

        protected override void Dispose(bool disposing)
        {
            foreach (var d in ReadinessChanged?.GetInvocationList() ?? Array.Empty<Delegate>())
            {
                ReadinessChanged -= d as ReadinessChangedEventHandler;
            }

            foreach (var d in Error?.GetInvocationList() ?? Array.Empty<Delegate>())
            {
                Error -= d as ErrorEventHandler;
            }

            foreach (var d in ShotMetrics?.GetInvocationList() ?? Array.Empty<Delegate>())
            {
                ShotMetrics -= d as MetricsEventHandler;
            }

            base.Dispose(disposing);
        }
    }
}