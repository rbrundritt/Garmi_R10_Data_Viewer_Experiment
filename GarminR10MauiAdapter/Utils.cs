using GarminR10MauiAdapter.OpenConnect;

namespace GarminR10MauiAdapter
{
    public static class Utils
    {
        #region Public Methods

        /// <summary>
        /// Converts speeds.
        /// </summary>
        /// <param name="speed">Speed value.</param>
        /// <param name="inputUnits">Units of input value.</param>
        /// <param name="outputUnits">Units of output value.</param>
        /// <returns></returns>
        public static float ConvertSpeed(float speed, SpeedUnit inputUnits, SpeedUnit outputUnits)
        {
            if (inputUnits.Equals(outputUnits))
            {
                return speed;
            }

            //Convert speed to meters per second as a base unit.
            switch (inputUnits)
            {
                case SpeedUnit.MPH:
                    speed *= 0.44704f;
                    break;
                case SpeedUnit.KPH:
                    speed *= 0.277777778f;
                    break;
                case SpeedUnit.MPS:
                default:
                    break;
            }

            //Convert meters per second to output units.
            switch (outputUnits)
            {
                case SpeedUnit.MPH:
                    speed *= 2.23693629f;
                    break;
                case SpeedUnit.KPH:
                    speed *= 3.6f;
                    break;
                case SpeedUnit.MPS:
                default:
                    break;
            }

            return speed;
        }

        /// <summary>
        /// Converts distances.
        /// </summary>
        /// <param name="distance">Distance value</param>
        /// <param name="inputUnits">Units of input value.</param>
        /// <param name="outputUnits">Units of output value.</param>
        /// <returns></returns>
        public static float ConvertDistance(float distance, DistanceUnit inputUnits, DistanceUnit outputUnits)
        {
            if (inputUnits.Equals(outputUnits))
            {
                return distance;
            }

            //Convert distance to meters as a base unit.
            switch (inputUnits)
            {
                case DistanceUnit.Feet:
                    distance *= 0.3048f;
                    break;
                case DistanceUnit.Yards:
                    distance *= 0.9144f;
                    break;
                case DistanceUnit.Meters:
                default:
                    break;
            }

            //Convert meters to output units.
            switch (outputUnits)
            {
                case DistanceUnit.Feet:
                    distance *= 3.2808399f;
                    break;
                case DistanceUnit.Yards:
                    distance *= 1.0936133f;
                    break;
                case DistanceUnit.Meters:
                default:
                    break;
            }

            return distance;
        }

        /// <summary>
        /// Converts temperatures from Celsius (metric) to Fahrenheit (Imperial) or vice versa.
        /// </summary>
        /// <param name="temperature">Temperature value.</param>
        /// <param name="inputUnits">Units of input value.</param>
        /// <param name="outputUnits">Units of output value.</param>
        /// <returns></returns>
        public static float ConvertTemperature(float temperature, TemperatureUnit inputUnits, TemperatureUnit outputUnits)
        {
            if (inputUnits.Equals(outputUnits))
            {
                return temperature;
            }
            else if (inputUnits.Equals(TemperatureUnit.Celsius))
            {
                //Celsius to Fahrenheit
                return temperature * 1.8f + 32;
            }

            //Fahrenheit to Celsius
            return (temperature - 32) / 1.8f;
        }

        /// <summary>
        /// Converts Club enum to a string value.
        /// </summary>
        private static Dictionary<Club, string> ClubNameMap = new Dictionary<Club, string>()
        {
            //Driver
            { Club.DR, "Driver" },

            //Woods
            { Club.W2, "2 Wood" },
            { Club.W3, "3 Wood" },
            { Club.W4, "4 Wood" },
            { Club.W5, "5 Wood" },
            { Club.W6, "6 Wood" },
            { Club.W7, "7 Wood" },

            //Hybrids
            { Club.H2, "2 Hybrid" },
            { Club.H3, "3 Hybrid" },
            { Club.H4, "4 Hybrid" },
            { Club.H5, "5 Hybrid" },
            { Club.H6, "6 Hybrid" },
            { Club.H7, "7 Hybrid" },

            //Irons
            { Club.I1, "1 Iron" },
            { Club.I2, "2 Iron" },
            { Club.I3, "3 Iron" },
            { Club.I4, "4 Iron" },
            { Club.I5, "5 Iron" },
            { Club.I6, "6 Iron" },
            { Club.I7, "7 Iron" },
            { Club.I8, "8 Iron" },
            { Club.I9, "9 Iron" },

            //Wedges
            { Club.PW, "Pitching Wedge" },
            { Club.GW, "Gap Wedge" },
            { Club.SW, "Sand Wedge" },
            { Club.LW, "Lob Wedge" },
            //Putter
            { Club.PT, "Putter" },

            //Unknown
            { Club.unknown, "" }
        };

        /// <summary>
        /// Returns the name of the club.
        /// </summary>
        /// <param name="club"></param>
        /// <returns></returns>
        public static string ClubName(Club? club)
        {
            if (club != null && ClubNameMap.ContainsKey(club.Value))
            {
                return ClubNameMap[club.Value];
            }

            return "";
        }


        #endregion

        #region Internal Methods

        /// <summary>
        /// List of charcters that are considered to be CSV injection characters. (https://owasp.org/www-community/attacks/CSV_Injection)
        /// </summary>
        private static char[] CsvInjectionCharacters { get; set; } = new[] { '=', '@', '+', '-', '\t', '\r' };

        /// <summary>
        /// Sanitize user entered string values to remove CSV injection characters, and wrap with quotes if necessary.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        internal static string SanitizeCsvField(string value)
        {
            //Check to see if the value is quoted. If so, remove the quotes and unescape any escaped quotes.
            if (value.StartsWith('"') && value.EndsWith('"'))
            {
                //Remove the quotes.
                value = value.Substring(1, value.Length - 2).Replace("\"\"", "\"");
            }

            //Check to see if the value starts with any of the injection characters
            if (value.IndexOfAny(CsvInjectionCharacters) == 0)
            {
                //If an injection character is detected, the field will be prepended with the ' character. The field will be quoted if it is not already.
                // =1+"2 -> "'=1+""2"
                return $"\"'{value.Replace("\"", "\"\"")}\"";
            }

            if (value.Contains('"') || value.Contains(",") || value.Contains("\n") || value.Contains("\r"))
            {
                //If the value contains a quote, comma, newline, or carriage return, the field will be quoted.
                return $"\"{value.Replace("\"", "\"\"")}\"";
            }

            return value;
        }

        #endregion
    }
}
