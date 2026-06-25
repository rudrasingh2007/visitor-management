using System;
using Microsoft.Extensions.Configuration;

namespace VisitorManagementSystem.Helpers
{
    public static class TimeZoneHelper
    {
        private static string _timeZoneId = "India Standard Time"; // Fallback default

        public static void Initialize(IConfiguration config)
        {
            var configZone = config["TimeZone"];
            if (!string.IsNullOrWhiteSpace(configZone))
            {
                _timeZoneId = configZone;
            }
        }

        public static DateTime ToAppLocalTime(this DateTime utcDateTime)
        {
            if (utcDateTime.Kind == DateTimeKind.Unspecified)
            {
                utcDateTime = DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);
            }

            try
            {
                var tz = TimeZoneInfo.FindSystemTimeZoneById(_timeZoneId);
                return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, tz);
            }
            catch (TimeZoneNotFoundException)
            {
                // Fallback to Server's Local Time if timezone ID is somehow invalid for the host OS
                return utcDateTime.ToLocalTime();
            }
            catch (InvalidTimeZoneException)
            {
                return utcDateTime.ToLocalTime();
            }
        }
    }
}
