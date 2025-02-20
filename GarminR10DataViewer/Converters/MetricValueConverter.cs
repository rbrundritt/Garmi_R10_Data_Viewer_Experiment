using System.Globalization;

namespace GarminR10DataViewer.Converters
{
    public class MetricValueConverter : IValueConverter
    {
        /// <summary>
        /// Number of decimal places to display.
        /// </summary>
        public int Decimals { get; set; } = 1;

        /// <summary>
        /// If the input is a directional value, the output will be displayed with an "L" or "R" suffix, and the value will be made positive.
        /// </summary>
        public bool IsDirectional { get; set; } = false;

        /// <summary>
        /// Unit symbol to display after the value.
        /// </summary>
        public string UnitSymbol { get; set; } = "";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return "-";
            }

            string direction = "R";

            if (value is float floatValue)
            {
                if (IsDirectional)
                {
                    if (floatValue < 1)
                    {
                        floatValue *= -1;
                        direction = "L";
                    }

                    return Math.Round(floatValue, Decimals).ToString("#,##0.##") + UnitSymbol + direction;
                }
                else
                {
                    return Math.Round(floatValue, Decimals).ToString("#,##0.##") + UnitSymbol;
                }
            }
            else if (value is double doubleValue)
            {
                if (IsDirectional)
                {
                    if (doubleValue < 1)
                    {
                        doubleValue *= -1;
                        direction = "L";
                    }

                    return Math.Round(doubleValue, Decimals).ToString("#,##0.##") + UnitSymbol + direction;
                }
                else
                {
                    return Math.Round(doubleValue, Decimals).ToString("#,##0.##") + UnitSymbol;
                }
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
