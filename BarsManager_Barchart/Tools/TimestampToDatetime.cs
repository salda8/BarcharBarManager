using System;

namespace BarsManager_Barchart.Tools
{
    internal class TimestampToDatetime
    {
        public static string UnixTimeStampToDateTime(double unixTimeStamp)
        {
            var dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToUniversalTime();
            var easternZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            var userTime = TimeZoneInfo.ConvertTimeFromUtc(dtDateTime, easternZone);
            return userTime.ToString("hh:mm:ss.fff");
        }
    }
}