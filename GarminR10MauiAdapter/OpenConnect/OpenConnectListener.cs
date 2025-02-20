using System.Diagnostics;
using System.Net.Sockets;
using System.Text.Json;

namespace GarminR10MauiAdapter.OpenConnect
{
    /// <summary>
    /// Listener for OpenConnect stream.
    /// Useful when two or more apps a
    /// </summary>
    public class OpenConnectListener : IDisposable
    {
        #region Events

        /// <summary>
        /// Event handler for when an OpenConnect message is received.
        /// </summary>
        public Action<OpenConnectApiMessage>? OnMessage;

        /// <summary>
        /// Event handler that is fired when player info has changed. Which hand they use, the club being used, distance to tee.
        /// </summary>
        public Action<PlayerInfo>? OnPlyerInfoChanged;

        #endregion

        #region Internal Properties

        internal TcpListener listener;

        internal bool isListening = false;

        // Indicator to stop listening
        internal bool stopListening = false;

        // tcp client callback
        internal AsyncCallback tcpClientCallback = new AsyncCallback(ConnectCallback);

        #endregion

        #region Constructors

        /// <summary>
        /// OpenConnect listener. Listens for data being sent from the OpenConnect API.
        /// Defaults to IP address: "127.0.0.1", and port: 921
        /// </summary>
        public OpenConnectListener()
        {
            listener = new TcpListener(System.Net.IPAddress.Parse("127.0.0.1"), 921);
        }

        /// <summary>
        /// OpenConnect listener. Listens for data being sent from the OpenConnect API.
        /// </summary>
        /// <param name="settings">Connection settings</param>
        public OpenConnectListener(OpenConnectSettings settings)
        {
            listener = new TcpListener(System.Net.IPAddress.Parse(settings.IpAddress ?? "127.0.0.1"), int.Parse(settings.Port ?? "921"));
        }

        #endregion

        #region Public Properties

        public bool IsListening { get { return isListening; } }

        #endregion

        #region Public Methods

        /// <summary>
        /// Dispose of the listener.
        /// </summary>
        public void Dispose()
        {
            listener.Dispose();
        }

        /// <summary>
        /// Start listening for incoming connections.
        /// </summary>
        /// <returns></returns>
        public bool Start()
        {
            //Check to see if already listening.
            if (isListening)
            {
                return true;
            }

            try {
                stopListening = false;

                listener.Start();

                // Initialize the tcp callback
                tcpClientCallback = new AsyncCallback(ConnectCallback);

                // Begin to accept tcp connections (async)
                AcceptConnectionsAysnc(this);

                isListening = true;

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return false;            
        }

        /// <summary>
        /// Stop listening for incoming connections.
        /// </summary>
        public void Stop()
        {
            stopListening = true;
            isListening = false;

            // Stop the listener.
            listener.Server.Close();
            listener.Stop();
        }

        #endregion

        #region Internal Static Methods

        /// <summary>
        /// Begin accepting connections async.
        /// </summary>
        /// <param name="openConnectListener"></param>
        internal static void AcceptConnectionsAysnc(OpenConnectListener openConnectListener)
        {
            // If listening indicator is true accept any connection triggering the connectCallback method.
            if (!openConnectListener.stopListening)
            {
                openConnectListener.listener.BeginAcceptTcpClient(openConnectListener.tcpClientCallback, openConnectListener);
            }
        }

        /// <summary>
        /// Async Connect callback to handle incoming accepted connections.
        /// </summary>
        /// <param name="result"></param>
        internal static void ConnectCallback(IAsyncResult result)
        {
            // Write debug message to screen
            Debug.WriteLine("Received connection request.");

            // Handle the incoming connection in the HandleReceive method as an own task.
            var openConnectListener = result.AsyncState as OpenConnectListener;

            if (openConnectListener != null)
            {
                var recieveTask = new Task(() =>
                {
                    HandleRecievedMessage(result, openConnectListener);
                });

                // Run the receiver task.
                recieveTask.Start();

                // Continue to accept incoming connections.
                AcceptConnectionsAysnc(openConnectListener);
            }
        }

        /// <summary>
        /// Handle an incoming connection.
        /// </summary>
        internal static void HandleRecievedMessage(IAsyncResult result, OpenConnectListener openConnectListener)
        {
            // Print debug message waiting for data
            Debug.WriteLine("Waiting for data.");

            // Interrupt this process once stop listenening is true
            if (openConnectListener.stopListening)
            {
                return;
            }
            
            // Accept connection
            using (var client = openConnectListener.listener.EndAcceptTcpClient(result))
            {
                client.NoDelay = true;

                // Get the data stream ftom the tcp client
                using (var stream = client.GetStream())
                {
                    using (var reader = new StreamReader(stream))
                    {
                        // Read data from sender.
                        string data = reader.ReadToEnd();

                        //Determine if the message is an OpenConnect shot or player message.
                        if (data.Contains("APIversion"))
                        {
                            // Print debug message.
                            Debug.WriteLine("Received OpenConnect message.");

                            try { 
                                // Deserialize the message.
                                var openConnectMessage = JsonSerializer.Deserialize<OpenConnectApiMessage>(data);

                                if (openConnectMessage != null)
                                {
                                    // Print debug message.
                                    Debug.WriteLine("Received message serialized.");

                                    // Invoke the OnMessage event.
                                    if (openConnectListener.OnMessage != null)
                                    {
                                        Task.Run(() => openConnectListener.OnMessage.Invoke(openConnectMessage));
                                    }                                    
                                } 
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine(ex.Message);
                            }
                        } 
                        else if(data.Contains("Player")) //Check to see if the message is related to player data.
                        {
                            // Print debug message
                            Debug.WriteLine("Received OpenConnect player message.");

                            try
                            {
                                // Deserialize the message
                                var openConnectResponse = JsonSerializer.Deserialize<OpenConnectApiResponse>(data);

                                if (openConnectResponse != null && openConnectResponse.Player != null)
                                {
                                    // Print debug message
                                    Debug.WriteLine("Received player update message.");

                                    // Invoke the OnPlyerInfoChanged event.
                                    if (openConnectListener.OnPlyerInfoChanged != null)
                                    {
                                        Task.Run(() => openConnectListener.OnPlyerInfoChanged.Invoke(openConnectResponse.Player));
                                    }
                                } 
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine(ex.Message);
                            }
                        }
                    }
                }
            }
        }

        #endregion
    }
}