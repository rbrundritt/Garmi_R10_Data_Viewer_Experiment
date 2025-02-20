//using GarminR10MauiAdapter.OpenConnect;
//using System.Text.Json;
//using System.Text.Json.Serialization;


//namespace GarminR10MauiAdapter
//{
//    public class ConnectionManager : IDisposable
//    {
//        //private OpenConnectClient? OpenConnectClient;
//        private BluetoothConnection? BluetoothConnection { get; }

//        public event ClubChangedEventHandler? ClubChanged;
//        public delegate void ClubChangedEventHandler(object sender, ClubChangedEventArgs e);

//        public event OnDeviceReadyEventHandler? OnDeviceReady;
//        public delegate void OnDeviceReadyEventHandler(object sender, OnDeviceReadyEventArgs e);

//        public class OnDeviceReadyEventArgs : EventArgs
//        {
//            public bool DeviceReady { get; set; }
//        }

//        public event OnShotEventHandler? OnShot;
//        public delegate void OnShotEventHandler(object sender, OnShotEventArgs e);

//        public class OnShotEventArgs : EventArgs
//        {
//            public Shot Shot { get; set; }
//        }

//        private JsonSerializerOptions serializerSettings = new JsonSerializerOptions()
//        {
//            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
//        };

//        public class ClubChangedEventArgs : EventArgs
//        {
//            public Club Club { get; set; }
//        }

//        private int shotNumber = 0;
//        private bool disposedValue;

//        public ConnectionManager(GerminR10Settings settings)
//        {
//            /*if (configuration.OpenConnect != null && configuration.OpenConnect.Mode != OpenConnectMode.Disabled)
//            {
//                OpenConnectClient = new OpenConnectClient(this, configuration.OpenConnect);
//                OpenConnectClient.ConnectAsync();
//            }*/

//            BluetoothConnection = new BluetoothConnection(this, settings);
//        }

//        internal void SendShot(OpenConnect.BallData? ballData, OpenConnect.ClubData? clubData)
//        {
//            shotNumber++;

//            Task.Run(() =>
//            {
//                OnShot?.Invoke(this, new OnShotEventArgs()
//                {
//                    Shot = new Shot()
//                    {
//                        ShotNumber = shotNumber,
//                        BallData = ballData,
//                        ClubData = clubData
//                    }
//                });
//            });

//            /* string openConnectMessage = JsonSerializer.Serialize(OpenConnectApiMessage.CreateShotData(
//               shotNumber++,
//               ballData,
//               clubData
//             ), serializerSettings);

//             OpenConnectClient.SendAsync(openConnectMessage);*/
//        }

//        public void ClubUpdate(Club club)
//        {
//            Task.Run(() =>
//            {
//                ClubChanged?.Invoke(this, new ClubChangedEventArgs()
//                {
//                    Club = club
//                });
//            });
//        }

//        internal void SendLaunchMonitorReadyUpdate(bool deviceReady)
//        {
//            Task.Run(() =>
//            {
//                OnDeviceReady?.Invoke(this, new OnDeviceReadyEventArgs()
//                {
//                    DeviceReady = deviceReady
//                });
//            });

//            // OpenConnectClient.SetDeviceReady(deviceReady);
//        }

//        protected virtual void Dispose(bool disposing)
//        {
//            if (!disposedValue)
//            {
//                if (disposing)
//                {
//                    BluetoothConnection?.Dispose();
//                    // OpenConnectClient?.DisconnectAndStop();
//                    //   OpenConnectClient?.Dispose();
//                }
//                disposedValue = true;
//            }
//        }

//        public void Dispose()
//        {
//            Dispose(disposing: true);
//            GC.SuppressFinalize(this);
//        }
//    }
//}