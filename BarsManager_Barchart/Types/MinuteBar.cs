using System;

namespace BarsManager_Barchart.Types
{
    public class MinuteBar
    {
        public decimal Close { get; set; }
        public decimal High { get; set; }
        public int Interval { get; set; }
        public decimal Low { get; set; }
        public decimal Open { get; set; }
        public string Symbol { get; set; }
        public long Volume { get; set; }
        public DateTime Time { get; set; }
    }
}