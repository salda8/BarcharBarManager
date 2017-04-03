using System;

namespace BarsManager_Barchart.Logic
{
    internal class TradingCalendar
    {
        public static bool TradingDay()
        {
            var tradingday = true;
            var dt = DateTime.Now;
            if (dt.DayOfWeek == DayOfWeek.Saturday || dt.DayOfWeek == DayOfWeek.Sunday)
            {
                Data.QuoteList.Clear();
                tradingday = false;
            }
            return tradingday;
        }

        public static bool RolloverDate()
        {
            if (DateTime.Today == DateTime.UtcNow) return true;
            return false;
        }
    }
}