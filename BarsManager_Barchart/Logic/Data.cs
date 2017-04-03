using System.Collections.Generic;
using System.Linq;
using BarsManager_Barchart.Types;
using ddfplus;
using MoreLinq;

namespace BarsManager_Barchart.Logic
{
    internal class Data
    {
        //public static List<Tick> TickList = new List<Tick>();

        //public static ObservableCollection<MinuteBars> BarList = new ObservableCollection<MinuteBars>();
        public static List<OHLCQuote> ListOhlc = new List<OHLCQuote>();
        public static List<byte[]> ListRaw = new List<byte[]>();
        public static List<Quote> QuoteList = new List<Quote>();
        public static List<BookQuote> BookList = new List<BookQuote>();
        public static int PriceMultiplier = 100000;
        public static List<MinuteBar> MinuteBars = new List<MinuteBar>();
        public static List<MinuteBar> FiveMinuteBars = new List<MinuteBar>();
        public static   List<MinuteBar> HourBars= new List<MinuteBar>();
            public static List<int> TimeframeList = new List<int> {1, 5, 10, 15, 30, 60, 120, 240, 360, 720, 1440};

        public static void OrderAndDistinctOnStartup()
        {
            var orderedlist = MinuteBars.OrderByDescending(x => x.Time).ToList();
            var distinct = orderedlist.DistinctBy(x => new { x.Symbol, x.Interval, x.Time }).OrderBy(x => x.Time).ToList();
            var noclosingbar = distinct.FindAll(x => x.Time.Hour!=16).ToList();
            var oneminute = noclosingbar.FindAll(x => x.Interval == 1).OrderBy(x => x.Time).ToList();
            var fiveminute = noclosingbar.FindAll(x => x.Interval == 5).OrderBy(x => x.Time).ToList();
            var sixtyminmute = noclosingbar.FindAll(x => x.Interval == 60).OrderBy(x => x.Time).ToList();
            FiveMinuteBars.AddRange(fiveminute);
            HourBars.AddRange(sixtyminmute);

            MinuteBars.Clear();
            MinuteBars.AddRange(oneminute);
            foreach (var bar in noclosingbar)
            {
                Program.Tf.StartNew((() => Db.AddNewBarInDb(bar)));
            }
        }
        public static void OrderAndDistinctOnNewDay()
        {
            Db.TruncateMinutebarsTable();
            var orderedlist = MinuteBars.OrderByDescending(x => x.Time).ToList();
            var distinct = orderedlist.DistinctBy(x => new { x.Symbol, x.Interval, x.Time }).OrderBy(x => x.Time).ToList();
            var sixtyminmute = distinct.FindAll(x => x.Interval == 60).OrderBy(x => x.Time).ToList();
            FiveMinuteBars.Clear();
            HourBars.Clear();
            HourBars.AddRange(sixtyminmute);
            MinuteBars.Clear();
            
            foreach (var bar in distinct)
            {
                Program.Tf.StartNew((() => Db.AddNewBarInDb(bar)));
            }
        }
    }
}