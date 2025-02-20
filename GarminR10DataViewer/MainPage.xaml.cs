using GarminR10DataViewer.ML;
using GarminR10DataViewer.Models;
using GarminR10MauiAdapter;
using GarminR10MauiAdapter.LaunchMonitors;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;

namespace GarminR10DataViewer
{
    public partial class MainPage : ContentPage
    {
        private GarminR10? launchMonitor;

        private GolfShotML GolfShotEnricher { get; set; }

        private MainPageViewModel viewModel;

        private int testShotNum = 0;

        public MainPage()
        {
            InitializeComponent();

            viewModel = new MainPageViewModel(this);
            BindingContext = viewModel;

            GolfShotEnricher = new GolfShotML();


            //https://enisn-projects.io/docs/en/uranium/latest/themes/material/components/DataGrid            
        }

        private void ConnectBtnClicked(object sender, EventArgs e)
        {
            StatusTbx.Text = "Connecting...";
            //ConnectBtn.IsVisible = false;

            if (launchMonitor == null)
            {
                launchMonitor = new GarminR10();
                launchMonitor.OnDeviceReady += GarminR10Connection_OnDeviceReady;
                launchMonitor.OnShot += GarminR10Connection_OnShot;
                launchMonitor.OnConnectionStatusChanged += (status) =>
                {
                    this.Dispatcher.Dispatch(() =>
                    {
                        StatusTbx.Text = status.ToString();

                        if(status == ConnectionStatus.Disconnected ||
                        status == ConnectionStatus.Device_Not_Found ||
                        status == ConnectionStatus.Max_Retries_Exceeded)
                        {
                            ConnectBtn.IsVisible = true;
                            DisconnectBtn.IsVisible = false;
                        } 
                        else if(status == ConnectionStatus.Connected)
                        {
                            ConnectBtn.IsVisible = false;
                            DisconnectBtn.IsVisible = true;
                        } 
                        else
                        {
                            ConnectBtn.IsVisible = false;
                            DisconnectBtn.IsVisible = false;
                        }
                    });
                };
            }

            launchMonitor.ConnectAsync();

           // DisconnectBtn.IsVisible = true;
        }

        private async void DisconnectBtnClicked(object sender, EventArgs e)
        {
            if (launchMonitor == null)
            {
                return;
            }
            
            await launchMonitor.DisconnectAsync();
        }

        public void GarminR10Connection_OnShot(LaunchMonitorShotData shot)
        {
            //Invoke on UI thread
            this.Dispatcher.Dispatch(() =>
            {
                viewModel.CurrentShotMLPredictions = GolfShotEnricher.GetPredictions(shot);
                viewModel.CurrentShot = shot;
            });
        }

        public void GarminR10Connection_OnDeviceReady(bool deviceReady)
        {
            this.Dispatcher.Dispatch(() =>
            {
                if(deviceReady)
                {
                    ReadyStatusTbx.Text ="Ready";
                    ReadyStatusIcon.BackgroundColor = Colors.Green;
                } 
                else
                {
                    ReadyStatusTbx.Text = "Not Ready";
                    ReadyStatusIcon.BackgroundColor = Colors.Red;
                }
            });
        }

        private void TestBtnClicked(object sender, EventArgs e)
        {
            //Generate a random shot.
            var rnd = new Random();

            testShotNum--;

            var ballSpeed = rnd.NextSingle() * 60 + 25;

            if(ImperialTab.IsChecked)
            {
                ballSpeed = rnd.NextSingle() * 135 + 50;
            }

            var smashFactor = 1.5f - rnd.NextSingle() * 0.5f;

            var testShot = new LaunchMonitorShotData()
            {
                ShotNumber = testShotNum,
                Units = ImperialTab.IsChecked ? Units.Imperial : Units.Metric,
                BallSpeed = ballSpeed,
                VerticalLaunchAngle = rnd.NextSingle() * 45,
                HorizontalLaunchAngle = rnd.NextSingle() * 60 - 30,
                SpinRate = rnd.NextSingle() * 13000,
                SpinAxis = rnd.NextSingle() * 170 - 85,
                ClubSpeed = ballSpeed / smashFactor,
                AngleOfAttack = rnd.NextSingle() * 5 - 3,
                BackswingTime = new TimeSpan(0, 0, 0, 3, rnd.Next(0, 999)),
                DownswingTime = new TimeSpan(0, 0, 0, 1, rnd.Next(0, 999)),
                Club = GarminR10MauiAdapter.OpenConnect.Club.DR,
                SwingType = SwingType.Normal,
                SpinMethod = SpinMethod.Estimated
            };

            viewModel.CurrentShotMLPredictions = GolfShotEnricher.GetPredictions(testShot);
            viewModel.CurrentShot = testShot;
        }

        private void ImperialTab_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            if(launchMonitor != null)
            {
                launchMonitor.OutputUnits = Units.Imperial;
            }
        }

        private void MetricTab_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            if (launchMonitor != null)
            {
                launchMonitor.OutputUnits = Units.Metric;
            }
        }

        private void ExportBtnClicked(object sender, EventArgs e)
        {
           
        }

        private async void TestConnectionBtnClicked(object sender, EventArgs e)
        {
            BluetoothDeviceInfo device = null;
            var picker = new BluetoothDevicePicker();
            device = await picker.PickSingleDeviceAsync();

            BluetoothClient client = new BluetoothClient();

            await foreach (var foundDevice in client.DiscoverDevicesAsync())
            {
                System.Diagnostics.Debug.WriteLine($"MAUI Discovered: {foundDevice.DeviceName} {foundDevice.DeviceAddress}");
            }
        }
    }
}
