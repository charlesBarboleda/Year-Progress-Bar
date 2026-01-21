using System;

namespace YearProgress
{
    /// <summary>
    /// Computes how much of the current year has elapsed.
    /// Returned value is a ratio in [0..1].
    /// </summary>
    public static class YearProgressCalculator
    {
        /// <summary>
        /// Returns elapsed progress for the year containing <paramref name="now"/>,
        /// as a normalized value in [0..1].
        /// Uses actual elapsed time between Jan 1 00:00 and Jan 1 next year 00:00,
        /// so leap years and DST transitions are handled correctly for Local times.
        /// </summary>
        public static double GetProgress01(DateTime now)
        {
            var startOfYear = new DateTime(now.Year, 1, 1, 0, 0, 0, now.Kind);
            var startOfNextYear = startOfYear.AddYears(1);

            if (now <= startOfYear) return 0.0;
            if (now >= startOfNextYear) return 1.0;

            var elapsed = now - startOfYear;
            var total = startOfNextYear - startOfYear;

            double progress01 = elapsed.TotalSeconds / total.TotalSeconds;
            return Math.Clamp(progress01, 0.0, 1.0);
        }

        // Convenience overload for current local time.
        public static double GetProgress01Now() => GetProgress01(DateTime.Now);

        // Returns progress as percent in [0..100].
        public static double GetProgressPercent(DateTime now) => GetProgress01(now) * 100.0;

        // Returns a formatted percent string (eg. "1.09%")
        public static string GetProgressPercentText(DateTime now, int decimals = 2)
        {
            double pct = GetProgressPercent(now);
            string format = "0." + new string('0', Math.Max(0, decimals));
            return pct.ToString(format) + "%";
        }

        // Returns how many seconds remain until the next year begins.
        // Useful if you want countdown text or tooltips.
        public static TimeSpan GetRemainingTime(DateTime now)
        {
            var startOfYear = new DateTime(now.Year, 1, 1, 0, 0, 0, now.Kind);
            var startOfNextYear = startOfYear.AddYears(1);

            if (now >= startOfNextYear) return TimeSpan.Zero;
            if (now <= startOfYear) return startOfNextYear - startOfYear;

            return startOfNextYear - now;
        }

        // Returns (elapsedDays, totalDays) where elapsedDays includes fractional day progress.
        public static (double elapsedDays, int totalDays) GetElapsedDaysInfo(DateTime now)
        {
            int totalDays = DateTime.IsLeapYear(now.Year) ? 366 : 365;

            // DayOfYear is 1-based, so subtract 1 for fully elapsed days before today.
            double elapsedDays = (now.DayOfYear - 1) + now.TimeOfDay.TotalDays;

            // Clamp just in case.
            if (elapsedDays < 0) elapsedDays = 0;
            if (elapsedDays > totalDays) elapsedDays = totalDays;

            return (elapsedDays, totalDays);
        }

    }
}
