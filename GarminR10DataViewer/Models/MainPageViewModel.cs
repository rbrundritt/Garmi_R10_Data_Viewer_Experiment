using GarminR10DataViewer.Controls;
using GarminR10MauiAdapter;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GarminR10DataViewer.Models
{
    public class MainPageViewModel: INotifyPropertyChanged
    {
        #region Priate Properties

        private ContentPage mainPage;
        private double metricsPanelWidth = 1000;
        private double metricsTableHeight = 1000;
        private double golfShotCanvasWidth = 400;
        private LaunchMonitorShotData? currentShot = null;
        private LaunchMonitorShotData? currentShotMLPredictions = null;

        public event PropertyChangedEventHandler? PropertyChanged;

        #endregion

        #region Constructor

        /// <summary>
        /// View model for the main page.
        /// </summary>
        /// <param name="page"></param>
        public MainPageViewModel(ContentPage page)
        {
            mainPage = page;
            Shots = new ObservableCollection<LaunchMonitorShotData>();

            //Monitor the page size to adjust the metrics panel layout.
            page.SizeChanged += Page_SizeChanged;
        }
        
        #endregion

        #region Properties

        /// <summary>
        /// A collection of all ML enriched shots
        /// </summary>
        public ObservableCollection<LaunchMonitorShotData> Shots { get; }

        /// <summary>
        /// The width of the metrics panel. Used to adjust the layout of the metrics panel based on the width of the page.
        /// </summary>
        public double MetricsPanelWidth
        {
            get => metricsPanelWidth;
            set
            {
                metricsPanelWidth = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MetricsPanelWidth)));
            }
        }


        /// <summary>
        /// The height of the metrics table. Used to adjust the layout of the metrics table based on the height of the page.
        /// </summary>
        public double MetricsTableHeight
        {
            get => metricsTableHeight;
            set
            {
                metricsTableHeight = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MetricsTableHeight)));
            }
        }

        /// <summary>
        /// The current shot being displayed in the metrics panel.
        /// </summary>
        public LaunchMonitorShotData? CurrentShot
        {
            get => currentShot;
            set
            {
                if (currentShot != value)
                {
                    currentShot = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentShot)));
                }
            }
        }

        /// <summary>
        /// The current shot being displayed in the metrics panel with ML predictions.
        /// </summary>
        public LaunchMonitorShotData? CurrentShotMLPredictions
        {
            get => currentShotMLPredictions;
            set
            {
                if (currentShotMLPredictions != value)
                {
                    currentShotMLPredictions = value;

                    //Add the enriched shot to the collection.
                    if (currentShotMLPredictions != null)
                    {
                        Shots.Add(currentShotMLPredictions);
                    }

                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentShotMLPredictions)));
                }
            }
        }

        #endregion

        #region Private Methods

        private void Page_SizeChanged(object? sender, EventArgs e)
        {
            var pageWidth = mainPage.Content.Width;

            if (pageWidth < 1000)
            {
                //Show stacked.
                MetricsPanelWidth = Math.Max(pageWidth - 60, 100);
            }
            else
            {
                MetricsPanelWidth = Math.Max(pageWidth - 100 - golfShotCanvasWidth, 100);
            }

            var pageHeight = mainPage.Content.Height;
            MetricsTableHeight = Math.Max(pageHeight - 150, 100);
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
