using System;

namespace BarsManager_Barchart.Types
{
    public class TestCore
    {
        public TestCore(string symbol, int number)
        {
            Symbol = Symbol;
            Time = DateTime.Now;
            Number = number;
        }

        public DateTime Time { get; set; }
        public string Symbol { get; set; }
        public int Number { get; set; }
    }
}