using System.Text.Json.Serialization;

namespace GarminR10MauiAdapter.OpenConnect
{
    //https://github.com/tnbozman/gspro-interface/blob/main/OpenAPI-Documentation-Feedback.MD
    //https://gsprogolf.com/GSProConnectV1.html
   
    /// <summary>
    /// Message to be sent to OpenConnect API.
    /// </summary>    
    public class OpenConnectApiMessage
    {
        /// <summary>
        /// Unique device ID.
        /// </summary>
        public string DeviceID { get; set; } = "GSPRO-R10";

        /// <summary>
        /// Units for the shot data. Can be "Yards" or "Meters".
        /// </summary>
        public string Units { get; set; } = "Yards";

        /// <summary>
        /// Shot number.
        /// </summary>
        public int ShotNumber { get; set; }

        /// <summary>
        /// OpenConnect API version.
        /// </summary>
        public const string APIVersion = "1"; 

        /// <summary>
        /// Ball data.
        /// </summary>
        public BallData? BallData { get; set; }

        /// <summary>
        /// Club data.
        /// </summary>
        public ClubData? ClubData { get; set; }

        /// <summary>
        /// Options for the shot data.
        /// </summary>
        public required ShotDataOptions ShotDataOptions { get; set; }
    }

    /// <summary>
    /// Shot data options. 
    /// </summary>
    public class ShotDataOptions
    {
        /// <summary>
        /// Indicates if the message contains ball data.
        /// </summary>
        public bool ContainsBallData { get; set; }

        /// <summary>
        /// Indicates if the message contains club data.
        /// </summary>
        public bool ContainsClubData { get; set; }

        /// <summary>
        /// Indicates if the launch monitor is ready.
        /// </summary>
        public bool? LaunchMonitorIsReady { get; set; }

        /// <summary>
        /// Indicates if the launch monitor detected a ball.
        /// </summary>
        public bool? LaunchMonitorBallDetected { get; set; }

        /// <summary>
        /// Indicates if the message is a heartbeat message.
        /// </summary>
        public bool? IsHeartBeat { get; set; }
    }

    /// <summary>
    /// Response object from OpenConnect API.
    /// </summary>
    public class OpenConnectApiResponse
    {
        /// <summary>
        /// Response code.
        /// </summary>
        public int Code { get; set; }

        /// <summary>
        /// Message for the response code.
        /// </summary>
        public string? Message { get; set; }
        
        /// <summary>
        /// Player information. Which hand they use, which club they are using, and the remaining distance to the target.
        /// </summary>
        public PlayerInfo? Player { get; set; }
    }

    /// <summary>
    /// Player information. Which hand they use, which club they are using, and the remaining distance to the target.
    /// </summary>
    public class PlayerInfo
    {
        /// <summary>
        /// Which hand the player hits with.
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public Handed? Handed { get; set; }

        /// <summary>
        /// Which club the player is using.
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public Club? Club { get; set; }

        /// <summary>
        /// Remaining distance to target in yards or meters depending on the specified units.
        /// </summary>
        public float? DistanceToTarget { get; set; }
    }

    /// <summary>
    /// Enum for the handedness of the player.
    /// </summary>
    public enum Handed
    {
        RH,
        LH
    }

    /// <summary>
    /// Golf club types.
    /// </summary>
    public enum Club
    {
        
        unknown,
        DR,
        W2,
        W3,
        W4,
        W5,
        W6,
        W7,
        I1,
        I2,
        I3,
        I4,
        I5,
        I6,
        I7,
        I8,
        I9,
        H2,
        H3,
        H4,
        H5,
        H6,
        H7,
        PW,
        GW,
        SW,
        LW,
        PT
    }
}