using Google.Protobuf;
using InTheHand.Bluetooth;
using LaunchMonitor.Proto;
using System.Diagnostics;
using System.Text;

namespace GarminR10MauiAdapter
{
    internal abstract class BaseDevice : IDisposable
    {
        #region Internal Properties

        private static Guid BATTERY_SERVICE_UUID = Guid.Parse("0000180f-0000-1000-8000-00805f9b34fb");
        private static Guid BATTERY_CHARACTERISTIC_UUID = Guid.Parse("00002a19-0000-1000-8000-00805f9b34fb");
        private static Guid DEVICE_INFO_SERVICE_UUID = Guid.Parse("0000180a-0000-1000-8000-00805f9b34fb");
        private static Guid FIRMWARE_CHARACTERISTIC_UUID = Guid.Parse("00002a28-0000-1000-8000-00805f9b34fb");
        private static Guid MODEL_CHARACTERISTIC_UUID = Guid.Parse("00002a24-0000-1000-8000-00805f9b34fb");
        private static Guid SERIAL_NUMBER_CHARACTERISTIC_UUID = Guid.Parse("00002a25-0000-1000-8000-00805f9b34fb");
        private static Guid DEVICE_INTERFACE_SERVICE = Guid.Parse("6A4E2800-667B-11E3-949A-0800200C9A66");
        private static Guid DEVICE_INTERFACE_NOTIFIER = Guid.Parse("6A4E2812-667B-11E3-949A-0800200C9A66");
        private static Guid DEVICE_INTERFACE_WRITER = Guid.Parse("6A4E2822-667B-11E3-949A-0800200C9A66");

        #endregion

        #region Public Properties

        public BluetoothDevice Device { get; }

        /// <summary>
        /// Battery life in percent
        /// </summary>
        public int Battery
        {
            get { return mBattery; }
            set
            {
                mBattery = value;
                BatteryLifeUpdated?.Invoke(this, new BatteryEventArgs() { Battery = value });
            }
        }

        /// <summary>
        /// Model name of the device
        /// </summary>
        public string? Model { get; private set; }

        /// <summary>
        /// Firmware version of the device
        /// </summary>
        public string? Firmware { get; private set; }

        /// <summary>
        /// Serial number of the device
        /// </summary>
        public string? Serial { get; private set; }

        #endregion

        #region Events

        /// <summary>
        /// Event raised when a message is recieved from the device
        /// </summary>
        public event MessageEventHandler? MessageRecieved;

        /// <summary>
        /// Event raised when a message is sent to the device
        /// </summary>
        public event MessageEventHandler? MessageSent;

        public delegate void MessageEventHandler(object sender, MessageEventArgs e);
        public class MessageEventArgs : EventArgs
        {
            public IMessage? Message { get; set; }
        }

        /// <summary>
        /// Event raised when the battery life is updated
        /// </summary>
        public event BatteryEventHandler? BatteryLifeUpdated;
        public delegate void BatteryEventHandler(object sender, BatteryEventArgs e);

        public class BatteryEventArgs : EventArgs
        {
            /// <summary>
            /// Battery life in percent
            /// </summary>
            public int Battery { get; set; }
        }

        #endregion

        #region Private Properties

        private EventWaitHandle mWriterSignal = new AutoResetEvent(false);
        private Queue<byte[]> mWriterQueue = new Queue<byte[]>();
        private EventWaitHandle mReaderSignal = new AutoResetEvent(false);
        private Queue<byte[]> mReaderQueue = new Queue<byte[]>();
        private EventWaitHandle mMsgProcessSignal = new AutoResetEvent(false);
        private Queue<byte[]> mMsgProcessQueue = new Queue<byte[]>();
        private ManualResetEventSlim mHandshakeCompleteResetEvent = new ManualResetEventSlim(false);
        private ManualResetEventSlim mProtoResponseResetEvent = new ManualResetEventSlim(false);
        private IMessage? mLastProtoReceived;
        private int mBattery;
        private bool mHandshakeComplete = false;
        private byte mHeader = 0x00;
        private int mProtoRequestCounter = 0;
        private CancellationTokenSource mCancellationToken;
        private Task mWriterTask;
        private Task mReaderTask;
        private Task mMsgProcessingTask;
        private GattCharacteristic? mGattWriter;
        private bool mDisposedValue;

        #endregion

        #region Constructor

        /// <summary>
        /// 
        /// </summary>
        /// <param name="device"></param>
        public BaseDevice(BluetoothDevice device)
        {
            Device = device;

            mCancellationToken = new CancellationTokenSource();
            mWriterTask = Task.Run(WriterThread, mCancellationToken.Token);
            mReaderTask = Task.Run(ReaderThread, mCancellationToken.Token);
            mMsgProcessingTask = Task.Run(MsgProcessingThread, mCancellationToken.Token);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Disposes the device
        /// </summary>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Sets up the device
        /// </summary>
        /// <returns></returns>
        public virtual bool Setup()
        {
            Debug.WriteLine($"Getting device info service");
            GattService deviceInfoService = Device.Gatt.GetPrimaryServiceAsync(DEVICE_INFO_SERVICE_UUID).WaitAsync(TimeSpan.FromSeconds(5)).Result;

            Debug.WriteLine($"Reading serial number");
            GattCharacteristic serialCharacteristic = deviceInfoService.GetCharacteristicAsync(SERIAL_NUMBER_CHARACTERISTIC_UUID).WaitAsync(TimeSpan.FromSeconds(5)).Result;
            Serial = Encoding.ASCII.GetString(serialCharacteristic.ReadValueAsync().WaitAsync(TimeSpan.FromSeconds(5)).Result);

            Debug.WriteLine($"Reading firmware version");
            GattCharacteristic firmwareCharacteristic = deviceInfoService.GetCharacteristicAsync(FIRMWARE_CHARACTERISTIC_UUID).WaitAsync(TimeSpan.FromSeconds(5)).Result;
            Firmware = Encoding.ASCII.GetString(firmwareCharacteristic.ReadValueAsync().WaitAsync(TimeSpan.FromSeconds(5)).Result);

            Debug.WriteLine($"Reading model name");
            GattCharacteristic modelCharacteristic = deviceInfoService.GetCharacteristicAsync(MODEL_CHARACTERISTIC_UUID).WaitAsync(TimeSpan.FromSeconds(5)).Result;
            Model = Encoding.ASCII.GetString(modelCharacteristic.ReadValueAsync().WaitAsync(TimeSpan.FromSeconds(5)).Result);

            Debug.WriteLine($"Reading battery life");
            GattService batteryService = Device.Gatt.GetPrimaryServiceAsync(BATTERY_SERVICE_UUID).WaitAsync(TimeSpan.FromSeconds(5)).Result;
            GattCharacteristic batteryCharacteristic = batteryService.GetCharacteristicAsync(BATTERY_CHARACTERISTIC_UUID).WaitAsync(TimeSpan.FromSeconds(5)).Result;
            batteryCharacteristic.CharacteristicValueChanged += (o, e) => Battery = e.Value[0];
            batteryCharacteristic.StartNotificationsAsync().Wait(TimeSpan.FromSeconds(5));

            Debug.WriteLine($"Setting up device interface service");
            GattService deviceInterfaceService = Device.Gatt.GetPrimaryServiceAsync(DEVICE_INTERFACE_SERVICE).WaitAsync(TimeSpan.FromSeconds(5)).Result;

            Debug.WriteLine($"Getting writer");
            mGattWriter = deviceInterfaceService.GetCharacteristicAsync(DEVICE_INTERFACE_WRITER).WaitAsync(TimeSpan.FromSeconds(5)).Result;

            Debug.WriteLine($"Getting reader");
            GattCharacteristic deviceInterfaceNotifier = deviceInterfaceService.GetCharacteristicAsync(DEVICE_INTERFACE_NOTIFIER).WaitAsync(TimeSpan.FromSeconds(5)).Result;
            deviceInterfaceNotifier.StartNotificationsAsync().Wait(TimeSpan.FromSeconds(5));
            deviceInterfaceNotifier.CharacteristicValueChanged += (o, e) => ReadBytes(e.Value);

            bool handshakeSuccess = PerformHandShake();
            if (!handshakeSuccess)
            {
                Debug.WriteLine("Failed handshake. Something went wrong in setup");
            }

            return handshakeSuccess;
        }

        /// <summary>
        /// Reads messages from the device
        /// </summary>
        private void ReaderThread()
        {
            var currentMessage = new List<byte>();

            while (!mCancellationToken.IsCancellationRequested)
            {
                if (mReaderQueue.Count > 0)
                {
                    IEnumerable<byte> msg = mReaderQueue.Dequeue();

                    byte header = msg.First();
                    msg = msg.Skip(1);

                    if (header == 0 || !mHandshakeComplete)
                    {
                        ContinueHandShake(msg);
                        continue;
                    }

                    bool readComplete = false;

                    if (msg.Last() == 0x00)
                    {
                        readComplete = true;
                        msg = msg.SkipLast(1);
                    }

                    if (msg.Count() > 0 && msg.First() == 0x00)
                    {
                        currentMessage.Clear();
                        msg = msg.Skip(1);
                    }

                    currentMessage.AddRange(msg);

                    if (readComplete && currentMessage.Count > 0)
                    {
                        byte[] decoded = COBS.Decode(currentMessage.ToArray()).ToArray();

                        mMsgProcessQueue.Enqueue(decoded);
                        mMsgProcessSignal.Set();
                        currentMessage.Clear();
                    }
                }
                else
                {
                    mReaderSignal.WaitOne(5000);
                }
            }
        }

        /// <summary>
        /// Writes messages to the device.
        /// </summary>
        private void WriterThread()
        {
            while (!mCancellationToken.IsCancellationRequested)
            {
                if (mWriterQueue.Count > 0)
                {
                    mGattWriter?.WriteValueWithResponseAsync(mWriterQueue.Dequeue());
                }
                else
                {
                    mWriterSignal.WaitOne(5000);
                }
            }
        }

        /// <summary>
        /// Processes messages.
        /// </summary>
        private void MsgProcessingThread()
        {
            while (!mCancellationToken.IsCancellationRequested)
            {
                if (mMsgProcessQueue.Count > 0)
                {
                    ProcessMessage(mMsgProcessQueue.Dequeue());
                }
                else
                {
                    mMsgProcessSignal.WaitOne(5000);
                }
            }
        }

        /// <summary>
        /// Performs a handshake with the device
        /// </summary>
        /// <returns></returns>
        public bool PerformHandShake()
        {
            Debug.WriteLine($"Starting handshake");
            mHandshakeComplete = false;
            mHandshakeCompleteResetEvent.Reset();
            mHeader = 0x00;
            SendBytes("000000000000000000010000");
            return mHandshakeCompleteResetEvent.Wait(TimeSpan.FromSeconds(10));
        }

        #endregion 

        #region Private Methods

        private void ContinueHandShake(IEnumerable<byte> msg)
        {
            string msgHex = msg.ToHexString();

            if (msgHex.StartsWith("010000000000000000010000"))
            {
                mHeader = msg.ElementAt(12);
                SendBytes("00");
                mHandshakeComplete = true;
                mHandshakeCompleteResetEvent.Set();
            }
        }

        private void ProcessMessage(byte[] frame)
        {
            if (BitConverter.ToUInt16(frame.SkipLast(2).Checksum()) != BitConverter.ToUInt16(frame.TakeLast(2).ToArray()))
            {
                Debug.WriteLine("CRC ERROR");
            }

            byte[] msg = frame.Skip(2).SkipLast(2).ToArray();
            string hex = msg.ToHexString();

            List<byte> ackBody = new List<byte>() { 0x00 };

            if (hex.StartsWith("A013"))
            {
                // device info
            }
            else if (hex.StartsWith("BA13"))
            {
                // config
            }
            else if (hex.StartsWith("B413")) // all protobuf responses
            {
                ushort counter = BitConverter.ToUInt16(msg[2..4]);
                ackBody.AddRange(msg[2..4]);
                ackBody.AddRange("00000000000000".ToByteArray());

                if (counter == mProtoRequestCounter)
                {
                    mLastProtoReceived = WrapperProto.Parser.ParseFrom(msg.Skip(16).ToArray());
                    MessageRecieved?.Invoke(this, new MessageEventArgs() { Message = mLastProtoReceived });
                    mProtoResponseResetEvent.Set();
                }
            }
            else if (hex.StartsWith("B313")) // all protobuf requests
            {
                ackBody.AddRange(msg[2..4]);
                ackBody.AddRange("00000000000000".ToByteArray());
                Task.Run(() =>
                {
                    var request = WrapperProto.Parser.ParseFrom(msg.Skip(16).ToArray());
                    MessageRecieved?.Invoke(this, new MessageEventArgs() { Message = request });
                    HandleProtobufRequest(request);
                });
            }

            AcknowledgeMessage(msg, ackBody);
        }

        public abstract void HandleProtobufRequest(IMessage request);

        private void AcknowledgeMessage(IEnumerable<byte> msg, IEnumerable<byte> respBody)
        {
            WriteMessage("8813".ToByteArray().Concat(msg.Take(2)).Concat(respBody).ToArray());
        }

        /// <summary>
        /// Sends a protobuf request to the device and waits for a response
        /// </summary>
        /// <param name="proto"></param>
        /// <returns></returns>
        internal IMessage? SendProtobufRequest(IMessage proto)
        {
            mProtoResponseResetEvent.Reset();

            byte[] bytes = proto.ToByteArray();
            int l = bytes.Length;
            byte[] fullMsg = "B313".ToByteArray()
              .Concat(BitConverter.GetBytes(mProtoRequestCounter))
              .Append<byte>(0x00)
              .Append<byte>(0x00)
              .Concat(BitConverter.GetBytes(l))
              .Concat(BitConverter.GetBytes(l))
              .Concat(bytes)
              .ToArray();

            WriteMessage(fullMsg);
            MessageSent?.Invoke(this, new MessageEventArgs() { Message = proto });
            if (mProtoResponseResetEvent.Wait(5000))
            {
                mProtoRequestCounter++;
                return mLastProtoReceived;
            }
            else
            {
                Debug.WriteLine($"Failed to get response for proto {mProtoRequestCounter}");
                return null;
            }
        }

        /// <summary>
        /// Reads bytes from the device
        /// </summary>
        /// <param name="bytes"></param>
        private void ReadBytes(byte[] bytes)
        { 
            mReaderQueue.Enqueue(bytes);
            mReaderSignal.Set();
        }

        /// <summary>
        /// Sends bytes to the device
        /// </summary>
        /// <param name="bytes"></param>
        private void SendBytes(IEnumerable<byte> bytes)
        {
            mWriterQueue.Enqueue(bytes.Prepend(mHeader).ToArray());
            mWriterSignal.Set();
        }

        /// <summary>
        /// Sends a string of hexadecimal bytes to the device
        /// </summary>
        /// <param name="hexBytes"></param>
        private void SendBytes(string hexBytes) => SendBytes(hexBytes.ToByteArray());

        /// <summary>
        /// Writes a message to the device
        /// </summary>
        /// <param name="bytes"></param>
        private void WriteMessage(byte[] bytes)
        {
            // Length of message + 2 bytes for length field + 2 bytes for crc field
            ushort length = (ushort)(2 + bytes.Length + 2);
            IEnumerable<byte> bytesWithLength = BitConverter.GetBytes(length).Concat(bytes);
            IEnumerable<byte> fullFrame = bytesWithLength.Concat(bytesWithLength.Checksum());

            List<byte> encoded = COBS.Encode(fullFrame).Prepend<byte>(0x00).Append<byte>(0x00).ToList();

            while (encoded.Count > 19)
            {
                SendBytes(encoded.Take(19));
                encoded = encoded.Skip(19).ToList();
            }
            if (encoded.Count > 0)
                SendBytes(encoded);
        }

        /// <summary>
        /// Disposes the device
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!mDisposedValue)
            {
                if (disposing)
                {
                    mCancellationToken.Cancel();
                    mWriterTask.Wait();
                    mReaderTask.Wait();
                    mMsgProcessingTask.Wait();

                    foreach (var d in MessageSent?.GetInvocationList() ?? Array.Empty<Delegate>())
                    {
                        MessageSent -= d as MessageEventHandler;
                    }

                    foreach (var d in MessageRecieved?.GetInvocationList() ?? Array.Empty<Delegate>())
                    {
                        MessageRecieved -= d as MessageEventHandler;
                    }

                    foreach (var d in BatteryLifeUpdated?.GetInvocationList() ?? Array.Empty<Delegate>())
                    {
                        BatteryLifeUpdated -= d as BatteryEventHandler;
                    }

                    Device?.Gatt?.Disconnect();
                }

                mDisposedValue = true;
            }
        }

        #endregion
    }
}