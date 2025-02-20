using GarminR10DataViewer.ML.Models;
using GarminR10MauiAdapter;
using Microsoft.ML;
using System.Diagnostics;

namespace GarminR10DataViewer.ML
{
    // All the code in this file is included in all platforms.
    public class GolfShotML
    {
        #region Private Static Properties

        private static readonly Dictionary<string, Lazy<PredictionEngine<GolfShotModelInput, GolfShotModelOutput>>> predictionEngines = new Dictionary<string, Lazy<PredictionEngine<GolfShotModelInput, GolfShotModelOutput>>>()
        {
            { "CarryDistance", GetLazyPredictionEngine("ML_Models/CarryDistanceML.mlnet") },
            { "CarryDistanceOffline", GetLazyPredictionEngine("ML_Models/CarryDistanceOfflineML.mlnet") },
            { "TotalDistance", GetLazyPredictionEngine("ML_Models/TotalDistanceML.mlnet") },
            { "TotalDistanceOffline", GetLazyPredictionEngine("ML_Models/TotalDistanceOfflineML.mlnet") },
            { "FlightTime", GetLazyPredictionEngine("ML_Models/FlightTimeML.mlnet") },
            { "BallSpeedAtImpact", GetLazyPredictionEngine("ML_Models/BallSpeedAtImpactML.mlnet") },
            { "DescentAngle", GetLazyPredictionEngine("ML_Models/DescentAngleML.mlnet") },
            { "MaxHeight", GetLazyPredictionEngine("ML_Models/MaxHeightML.mlnet") },
            { "MaxHeightDistance", GetLazyPredictionEngine("ML_Models/MaxHeightDistanceML.mlnet") },
            { "MaxHeightDistanceOffline", GetLazyPredictionEngine("ML_Models/MaxHeightDistanceOfflineML.mlnet") }
        };

        #endregion

        #region Public Methods

        /// <summary>
        /// Get shot predictions and enrich the shot data with the predictions.
        /// </summary>
        /// <param name="shot"></param>
        public void EnrichShotData(LaunchMonitorShotData shot)
        {
            var predictions = GetPredictions(shot);

            if (predictions != null)
            {

                if (predictions.CarryDistance != null)
                {
                    shot.CarryDistance = predictions.CarryDistance.Value;
                }

                if (predictions.CarryDistanceOffline != null)
                {
                    shot.CarryDistanceOffline = predictions.CarryDistanceOffline.Value;
                }

                if (predictions.TotalDistance != null)
                {
                    shot.TotalDistance = predictions.TotalDistance.Value;
                }

                if (predictions.TotalDistanceOffline != null)
                {
                    shot.TotalDistanceOffline = predictions.TotalDistanceOffline.Value;
                }

                if (predictions.MaxHeight != null)
                {
                    shot.MaxHeight = predictions.MaxHeight.Value;
                }

                if (predictions.MaxHeightDistance != null)
                {
                    shot.MaxHeightDistance = predictions.MaxHeightDistance.Value;
                }

                if (predictions.MaxHeightDistanceOffline != null)
                {
                    shot.MaxHeightDistanceOffline = predictions.MaxHeightDistanceOffline.Value;
                }

                if (predictions.DescentAngle != null)
                {
                    shot.DescentAngle = predictions.DescentAngle.Value;
                }

                if (predictions.FlightTime != null)
                {
                    shot.FlightTime = predictions.FlightTime.Value;
                }

                if (predictions.BallSpeedAtImpact != null)
                {
                    shot.BallSpeedAtImpact = predictions.BallSpeedAtImpact.Value;
                }
            }
        }

        /// <summary>
        /// Gets shot predictions; carry distance, carry lateral distance.
        /// </summary>
        /// <param name="shot"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public LaunchMonitorShotData? GetPredictions(LaunchMonitorShotData shot)
        {
            if (shot.BallSpeed == null)
            {
                Debug.WriteLine("Ball speed is required to enrich the shot with ML data");
                return null;
            }

            if (shot.HorizontalLaunchAngle == null)
            {
                Debug.WriteLine("Ball horizontal launch angle is required to enrich the shot with ML data");
                return null;
            }

            if (shot.VerticalLaunchAngle == null)
            {
                Debug.WriteLine("Ball vertical launch angle is required to enrich the shot with ML data");
                return null;
            }

            if (shot.SpinAxis == null)
            {
                Debug.WriteLine("Ball spin axis is required to enrich the shot with ML data");
                return null;
            }

            if (shot.SpinRate == null)
            {
                Debug.WriteLine("Ball spin rate is required to enrich the shot with ML data");
                return null;
            }

            var modelInput = new GolfShotModelInput(shot);

            //Clone the input shot to avoid modifying the original shot.
            var shotPrediction = shot.Clone();

            foreach(var key in predictionEngines.Keys)
            {
                var predictionEngine = predictionEngines[key];
                var prediction = predictionEngine.Value.Predict(modelInput);

                if (prediction != null)
                {
                    switch (key)
                    {
                        case "CarryDistance":
                            shotPrediction.CarryDistance = Utils.ConvertDistance(Clamp(prediction.Score, 0, 400), DistanceUnit.Meters, shot.DistanceUnits);
                            break;
                        case "CarryDistanceOffline":
                            shotPrediction.CarryDistanceOffline = Utils.ConvertDistance(Clamp(prediction.Score, -150, 150), DistanceUnit.Meters, shot.DistanceUnits);
                            break;
                        case "TotalDistance":
                            shotPrediction.TotalDistance = Utils.ConvertDistance(Clamp(prediction.Score, 0, 400), DistanceUnit.Meters, shot.DistanceUnits);
                            break;
                        case "TotalDistanceOffline":
                            shotPrediction.TotalDistanceOffline = Utils.ConvertDistance(Clamp(prediction.Score, -150, 150), DistanceUnit.Meters, shot.DistanceUnits);
                            break;
                        case "FlightTime":
                            shotPrediction.FlightTime = TimeSpan.FromSeconds(Clamp(prediction.Score, 0, 20));
                            break;
                        case "BallSpeedAtImpact":
                            shotPrediction.BallSpeedAtImpact = Utils.ConvertSpeed(Clamp(prediction.Score, 0, 200), SpeedUnit.MPS, shot.SpeedUnits);
                            break;
                        case "DescentAngle":
                            shotPrediction.DescentAngle = Clamp(prediction.Score, 0, 90); //Anything greater than 90 would likely occur when hitting into a head wind. ML model shouldn't get into this range.
                            break;
                        case "MaxHeight":
                            shotPrediction.MaxHeight = Utils.ConvertDistance(Clamp(prediction.Score, 0, 70), DistanceUnit.Meters, shot.HeightUnits);
                            break;
                        case "MaxHeightDistance":
                            shotPrediction.MaxHeightDistance = Utils.ConvertDistance(Clamp(prediction.Score, 0, 250), DistanceUnit.Meters, shot.DistanceUnits);
                            break;
                        case "MaxHeightDistanceOffline":
                            shotPrediction.MaxHeightDistanceOffline = Utils.ConvertDistance(Clamp(prediction.Score,-150, 150), DistanceUnit.Meters, shot.DistanceUnits);
                            break;
                    }
                }
            }

            return shotPrediction;
        }

        #endregion

        /// <summary>
        /// Lazy loads a pre-trained model from the specified path.
        /// </summary>
        /// <param name="modelPath"></param>
        /// <returns></returns>
        private static Lazy<PredictionEngine<GolfShotModelInput, GolfShotModelOutput>> GetLazyPredictionEngine(string modelPath)
        {
            return new Lazy<PredictionEngine<GolfShotModelInput, GolfShotModelOutput>>(() => LoadPredictionEngine(modelPath), true);
        }

        /// <summary>
        /// Loads a pre-trained model from the specified path.
        /// </summary>
        /// <param name="modelPath">The path to the model.</param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException"></exception>
        private static PredictionEngine<GolfShotModelInput, GolfShotModelOutput> LoadPredictionEngine(string modelPath)
        {
            if (!FileSystem.Current.AppPackageFileExistsAsync(modelPath).Result)
            {
                throw new FileNotFoundException($"{modelPath} not found");
            }

            using (var fileStream = FileSystem.Current.OpenAppPackageFileAsync(modelPath).Result)
            {
                var mlContext = new MLContext();
                ITransformer mlModel = mlContext.Model.Load(fileStream, out var _);
                return mlContext.Model.CreatePredictionEngine<GolfShotModelInput, GolfShotModelOutput>(mlModel);
            }
        }

        /// <summary>
        /// Clamp a value within a range.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        private static float Clamp(float value, float min, float max)
        {
            return Math.Max(min, Math.Min(max, value));
        }
    }
}
