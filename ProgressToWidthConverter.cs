using System.Globalization;
using System.Windows.Data;

namespace YearProgress
{
    public sealed class ProgressToWidthConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length != 4) return 0.0;

            if (values[0] is not double actualWidth ||
                values[1] is not double min ||
                values[2] is not double max ||
                values[3] is not double val)
                return 0.0;

            double range = Math.Max(1e-9, max - min);
            double t = (val - min) / range;
            t = Math.Clamp(t, 0.0, 1.0);

            // Keep inside borders
            return actualWidth * t;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
