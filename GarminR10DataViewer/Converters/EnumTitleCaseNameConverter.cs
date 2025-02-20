using System.Globalization;

namespace GarminR10DataViewer.Converters
{
    internal class EnumTitleCaseNameConverter : IValueConverter
    {
        private static TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return value;
            }

            var type = value.GetType();
            if (type.IsEnum)
            {
                // Get the name of the enum value can title case it.
                var name = Enum.GetName(type, value);
                if (name != null)
                {
                    return textInfo.ToTitleCase(name.Replace("_", " "));
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
