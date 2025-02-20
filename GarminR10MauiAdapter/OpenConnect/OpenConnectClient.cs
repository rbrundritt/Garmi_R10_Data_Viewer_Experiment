using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using TcpClient = NetCoreServer.TcpClient;

namespace GarminR10MauiAdapter.OpenConnect
{
    /// <summary>
    /// Open connect client. Streams data to and from the OpenConnect api.
    /// </summary>
    public class OpenConnectClient : TcpClient
    {
        // https://github.com/tnbozman/gspro-interface/blob/main/OpenAPI-Documentation-Feedback.MD
        // https://gsprogolf.com/GSProConnectV1.html

        #region Events

        /// <summary>
        /// Event handler that is fired when player info has changed. Which hand they use, the club being used, distance to tee.
        /// </summary>
        public Action<PlayerInfo>? OnPlyerInfoChanged;

        #endregion

        #region Private Properties

        private Timer? PingTimer;

        /// <summary>
        /// Flag to indicate if the client was initially connected.
        /// </summary>
        private bool initiallyConnected;

        /// <summary>
        /// Flag to stop the client from reconnecting.
        /// </summary>
        private bool stopReconnecting;

        /// <summary>
        /// Settings for the JSON serializer.
        /// </summary>
        private JsonSerializerOptions serializerSettings = new JsonSerializerOptions()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        #endregion

        #region Constructors

        /// <summary>
        /// Open connect client. Streams data to and from the OpenConnect api.
        /// Defaults to IP address: "127.0.0.1", and port: 921
        /// </summary>
        public OpenConnectClient() : base("127.0.0.1", 921)
        {
        }

        /// <summary>
        /// Open connect client. Streams data to and from the OpenConnect api.
        /// </summary>
        /// <param name="settings">Connection settings</param>
        public OpenConnectClient(OpenConnectSettings settings)
          : base(settings.IpAddress ?? "127.0.0.1", int.Parse(settings.Port ?? "921"))
        {
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// The last known player info recieved from OpenConnect API.
        /// </summary>
        public PlayerInfo? LastKnownPlayerInfo { get; internal set; } = null;

        #endregion

        #region Public Methods

        /// <summary>
        /// Disconnects and stops the client.
        /// </summary>
        public void DisconnectAndStop()
        {
            stopReconnecting = true;
            DisconnectAsync();

            while (IsConnected)
            {
                Thread.Yield();
            }
        }

        /// <summary>
        /// Sends a device ready message.
        /// </summary>
        /// <param name="deviceReady"></param>
        public void SetDeviceReady(bool deviceReady)
        {
            SendAsync(CreateHeartbeat(deviceReady));
        }
        
        /// <summary>
        /// Connects to the OpenConnect api.
        /// </summary>
        /// <returns></returns>
        public override bool ConnectAsync()
        {
            Debug.WriteLine($"Connecting to OpenConnect api ({Address}:{Port})...");
            return base.ConnectAsync();
        }

        /// <summary>
        /// Sends a message.
        /// </summary>
        /// <param name="message"></param>
        /// <returns>Returns true if data sent successfully.</returns>
        public override bool SendAsync(string message)
        {
            Debug.WriteLine(message);
            return base.SendAsync(message);
        }

        /// <summary>
        /// Sends a message.
        /// </summary>
        /// <param name="message">OpenConnect message to send.</param>
        /// <returns>Returns true if data sent successfully.</returns>
        public bool SendAsync(OpenConnectApiMessage message)
        {
            return SendAsync(JsonSerializer.Serialize(message, serializerSettings));
        }

        /// <summary>
        /// Sends shot data to the OpenConnect api.
        /// </summary>
        /// <param name="shotNumber">Unique shot number</param>
        /// <param name="ballData">Ball data</param>
        /// <param name="clubData">Club data</param>
        /// <param name="units">Units of measurement the data is in.</param>
        /// <returns>Returns true if data sent successfully.</returns>
        //public bool SendShotData(int shotNumber, BallData ballData, ClubData? clubData = null, Units? units = GarminR10MauiAdapter.Units.Imperial)
        //{
        //    return SendAsync(new OpenConnectApiMessage()
        //    {
        //        Units = units == GarminR10MauiAdapter.Units.Imperial ? "Yards" : "Meters",
        //        ShotNumber = shotNumber,
        //        BallData = ballData,
        //        ClubData = clubData,
        //        ShotDataOptions = new ShotDataOptions()
        //        {
        //            ContainsBallData = (ballData != null),
        //            ContainsClubData = (clubData != null),
        //        }
        //    });
        //}

        /// <summary>
        /// Sends a test shot to the OpenConnect api.
        /// </summary>
        /// <returns>Returns true if data sent successfully.</returns>
        public bool SendTestShot()
        {
            return SendAsync(new OpenConnectApiMessage()
            {
                Units = "Yards",
                ShotNumber = 0,
                BallData = new BallData()
                {
                    Speed = 200,
                    SpinAxis = -90,
                    TotalSpin = 50000,
                    SideSpin = 50000,
                    BackSpin = -100000,
                    HLA = 10,
                    VLA = 20
                },
                ShotDataOptions = new ShotDataOptions()
                {
                    ContainsBallData = true,
                    ContainsClubData = false,
                }
            });
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Event handler for when the client is connected.
        /// </summary>
        protected override void OnConnected()
        {
            initiallyConnected = true;

            Debug.WriteLine($"TCP client connected a new session with Id {Id}.");

            PingTimer = new Timer(SendPing, null, 0, 0);
        }

        /// <summary>
        /// Event handler for when the client receives a message.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="size"></param>
        protected override void OnReceived(byte[] buffer, long offset, long size)
        {
            string received = Encoding.UTF8.GetString(buffer, (int)offset, (int)size);

            Debug.WriteLine(received);

            // Sometimes multiple responses received in one buffer. Convert to list format to handle
            // ie "{one}{two}" => "[{one},{two}]"
            string listReceived = $"[{received.Replace("}{", "},{")}]";
            try
            {
                List<OpenConnectApiResponse> responses = JsonSerializer.Deserialize<List<OpenConnectApiResponse>>(listReceived) ?? new List<OpenConnectApiResponse>();
                foreach (OpenConnectApiResponse resp in responses)
                {
                    // Invoke the OnPlyerInfoChanged event.
                    if (resp.Player != null && OnPlyerInfoChanged != null)
                    {
                        if(resp.Player.Club != null)
                        {
                            LastKnownPlayerInfo = resp.Player;
                        }

                        Task.Run(() => OnPlyerInfoChanged.Invoke(resp.Player));
                    }
                }
            }
            catch
            {
                Debug.WriteLine("Error parsing response.");
            }
        }

        /// <summary>
        /// Called when the client has an error.
        /// </summary>
        /// <param name="error"></param>
        protected override void OnError(SocketError error)
        {
            if (error != SocketError.TimedOut)
            {
                Debug.WriteLine($"TCP client caught an error with code {error}.");
            }
        }

        /// <summary>
        /// Called when the client is disconnected. Tries to reconnect.
        /// </summary>
        protected override void OnDisconnected()
        {
            if (initiallyConnected)
            {
                Debug.WriteLine($"TCP client disconnected a session with Id {Id}.");
            }

            Thread.Sleep(5000);

            if (!stopReconnecting)
            {
                ConnectAsync();
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Pings the device with a heartbeat message.
        /// </summary>
        /// <param name="state"></param>
        private void SendPing(object? state)
        {
            SendAsync(CreateHeartbeat(false));
        }

        /// <summary>
        /// Creates a heart beat message.
        /// </summary>
        /// <param name="deviceReady">Indicates if the device is ready to measure a swing.</param>
        /// <returns>OpenCOnnect message with heart beat info.</returns>
        private static OpenConnectApiMessage CreateHeartbeat(bool deviceReady)
        {
            return new OpenConnectApiMessage()
            {
                ShotNumber = 0,
                ShotDataOptions = new ShotDataOptions()
                {
                    ContainsBallData = false,
                    ContainsClubData = false,
                    IsHeartBeat = true,
                    LaunchMonitorIsReady = deviceReady
                }
            };
        }

        #endregion
    }
}
