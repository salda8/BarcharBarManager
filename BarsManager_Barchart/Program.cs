using BarsManager_Barchart.Backend;
using BarsManager_Barchart.Logic;
using BarsManager_Barchart.Types;
using ddfplus;
using ddfplus.Net;
using MoreLinq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace BarsManager_Barchart
{
    internal class Program
    {
        //public static List<Tick> Data.QuoteList = new List<Tick>();
        //public static List<MinuteBars> Data.ListOhlc = new List<MinuteBars>();
        public static TaskFactory Tf = new TaskFactory();

        public static int Variable;
        public static int Variable1 = 0;
        public static int RealtimeBarsCount = 0;
        public static List<TestCore> TestList = new List<TestCore>();
        public static int CloseMinute = 59;
        public static int CloseHour = 15;

        public static int Main(string[] args)
        {
            Thread.Sleep(10000);
            StartTrading();
            return -1;
        }

        //Thread.Sleep(100);
        public static void Barchart(Core core)
        {
            Thread.Sleep(1000);
            if (core.Status == Status.Disconnected || core.Status == Status.Error)
            {
                // Set connection properties prior to connecting
                core.Connect();
            }
            core.Client.Symbols = MyContracts.SymbolList;
        }

        private static void StartTrading()
        {
            Db.TruncateMinutebarsTable();
            var core = new Core();
            MyContracts.GetTickerId(core);
            Data.OrderAndDistinctOnStartup();
            Variable++;

            while (true)
            {
                var date = DateTime.Now.AddHours(-7);
                if (date.Second == 0)
                {
                    if (date.Hour == 16)
                    {
                        MyContracts.ClearContracts(core);

                        while (DateTime.UtcNow.Minute != 58)
                        {
                            Thread.Sleep(1);
                        }
                        //Close existing connection
                        Connection.Close();
                        // Starts a new instance of the program itself
                        var fileName = Assembly.GetExecutingAssembly().Location;
                        System.Diagnostics.Process.Start(fileName);
                        // Closes the current process
                        Environment.Exit(0);
                    }
                    else if (date.Hour == 15 && date.Minute == 59)
                    {
                        foreach (var symbol in MyContracts.ContractsList)
                        {
                            Tf.StartNew((() => Parallel.Invoke(() => StartGettingBars(symbol, date))));
                            //Parallel.Invoke((() => TestStartGettingBars(symbol)));
                            //Tf.StartNew((() => StartGettingBars(symbol)));
                        }
                    }
                    else if (date.Minute == 0 || date.Minute % 5 == 0)
                    {
                        foreach (var symbol in MyContracts.ContractsList)
                        {
                            Tf.StartNew((() => Parallel.Invoke(() => StartGettingBars(symbol, date))));
                            //Parallel.Invoke((() => TestStartGettingBars(symbol)));
                            //Tf.StartNew((() => StartGettingBars(symbol)));
                        }
                    }

                    Thread.Sleep(2000);
                }
                Thread.Sleep(1);
            }
        }

        private static void StartGettingBars(string tickerId, DateTime date)
        {
            

            ConvertToFiveMinuteBar(tickerId, 5, date);
        
        }

    private static void HourAndMoreBars(string tickerId, int date)
        {
            //var date = IbClient._lastDateTime.Adddate(-1).Hour;
            if (date % 4 == 0 && date % 6 == 0)
            {
                Parallel.Invoke(() => ConvertToSixtyMinuteBar(tickerId, 120));
                Parallel.Invoke(() => ConvertToSixtyMinuteBar(tickerId, 240));
                Parallel.Invoke(() => ConvertToSixtyMinuteBar(tickerId, 360));
            }
            else if (date % 2 == 0 && date % 4 == 0)
            {
                Parallel.Invoke(() => ConvertToSixtyMinuteBar(tickerId, 120));
                Parallel.Invoke(() => ConvertToSixtyMinuteBar(tickerId, 240));
            }
            else if (date % 2 == 0 && date % 6 == 0)
            {
                Parallel.Invoke(() => ConvertToSixtyMinuteBar(tickerId, 120));
                Parallel.Invoke(() => ConvertToSixtyMinuteBar(tickerId, 360));
            }
            else if (date % 2 == 0)
            {
                Parallel.Invoke(() => ConvertToSixtyMinuteBar(tickerId, 120));
            }
            if (date % 12 == 0)
            {
                Parallel.Invoke(() => ConvertToSixtyMinuteBar(tickerId, 720));
            }
        }

        /// <summary>
        ///     Converts to more minute bar.
        /// </summary>
        /// <param name="tickerId"></param>
        /// <param name="tf">The tf.</param>
        /// <param name="dt"></param>
        private static void ConvertToMoreMinuteBar(string tickerId, int tf, DateTime dt)
        {
            var tickerIdd = tickerId.Substring(0, 2);

            if (tickerIdd != string.Empty)
            {
                var fiveMinuteBars = Data.FiveMinuteBars.FindAll(x => x.Symbol == tickerId).TakeLast(tf / 5).ToList();

                // fiveminutebars = fiveminutebars.TakeLast(tf / 5).ToList();

                if (fiveMinuteBars.Count == 12)
                {
                    var bar = ConvertListToBar(fiveMinuteBars, tickerId, tf);
                    if (bar == null) throw new ArgumentNullException(nameof(bar));
                    Data.HourBars.Add(bar);
                    Db.AddNewBarInDb(bar);
                }
                else if (fiveMinuteBars.Count == tf / 5)
                {
                    var bar = ConvertListToBar(fiveMinuteBars, tickerId, tf);
                    if (bar == null) throw new ArgumentNullException(nameof(bar));
                    //Data.FiveMinuteBars.Add(bar);

                    Db.AddNewBarInDb(bar);
                }
            }
            else
            {
                Console.WriteLine("Ticker: " + tickerId + "/" + tf + " ma Z€R0 baru!");
            }
        }

        /// <summary>
        /// Converts to five minute bar.
        /// </summary>
        /// <param name="tickerId">The ticker identifier.</param>
        /// <param name="tf">The tf.</param>
        /// <param name="dt">The dt.</param>
        /// <exception cref="ArgumentNullException"></exception>
        private static void ConvertToFiveMinuteBar(string tickerId, int tf, DateTime dt)
        {
            var tickerIdd = tickerId.Substring(0, 2);
            if (tickerIdd != string.Empty)
            {
                var find = Data.MinuteBars.FindLast(
                    x => x.Symbol == tickerId && x.Time.Minute == dt.AddMinutes(-1).Minute && x.Time.Hour == dt.AddMinutes(-1).Hour);

                //while (find == null)
                //{
                //    find = Data.MinuteBars.FindLast(
                //   x => x.Symbol == tickerId && x.Time.Minute == dt.AddMinutes(-1).Minute && x.Time.Hour == dt.AddMinutes(-1).Hour);
                //    Thread.Sleep(1);
                //}
                var i = 0;
                for (i = 0; i < 60000; i++)
                {
                    if (find == null)
                    {
                        find = Data.MinuteBars.FindLast(
                         x => x.Symbol == tickerId && x.Time.Minute == dt.AddMinutes(-1).Minute && x.Time.Hour == dt.AddMinutes(-1).Hour);
                        Thread.Sleep(1);
                    }
                    else
                    {
                        break;
                    }
                }
                if (i != 60000)
                {
                    if (dt.Minute == CloseMinute && dt.Hour == CloseHour)
                    {
                        tf = tf - 1;
                    }
                    var minutebars = Data.MinuteBars.FindAll(x => x.Symbol == tickerId).TakeLast(tf).ToList();

                    if (minutebars.Count == tf)
                    {
                        var bar = ConvertListToBar(minutebars, tickerId, tf);
                        if (bar == null) throw new ArgumentNullException(nameof(bar));
                        if (dt.Minute == CloseMinute && dt.Hour == CloseHour)
                        {
                            bar.Interval = bar.Interval + 1;
                        }
                        Data.FiveMinuteBars.Add(bar);

                        CheckToMakeOtherBars(dt, tickerId);
                        Db.AddNewBarInDb(bar);
                    }
                    else
                    {
                        Console.WriteLine("Ticker " + tickerId + "/" + tf + " ma Z€R0 baru!");
                    }
                }
            }
        }

        private static void CheckToMakeOtherBars(DateTime date, string tickerId)
        {
            if (date.Minute == 0)
            {
                Parallel.Invoke(() => { ConvertToMoreMinuteBar(tickerId, 60, date); });
                Parallel.Invoke(() => { ConvertToMoreMinuteBar(tickerId, 10, date); });
                Parallel.Invoke(() => { ConvertToMoreMinuteBar(tickerId, 15, date); });
                Parallel.Invoke(() => { ConvertToMoreMinuteBar(tickerId, 30, date); });
                //ConvertToMoreMinuteBar(tickerId, 60, date);
                //ConvertToMoreMinuteBar(tickerId, 10, date);
                //ConvertToMoreMinuteBar(tickerId, 15, date);
                //ConvertToMoreMinuteBar(tickerId, 30, date);

                Tf.StartNew((() => { HourAndMoreBars(tickerId, date.Hour); }));
            }
            else if (date.Minute == 30)
            {
                // Parallel.Invoke(() => ConvertToMoreMinuteBar(tickerId, 5));
                Parallel.Invoke(() => ConvertToMoreMinuteBar(tickerId, 15, date));
                Parallel.Invoke(() => ConvertToMoreMinuteBar(tickerId, 30, date));
                Parallel.Invoke(() => ConvertToMoreMinuteBar(tickerId, 10, date));
            }
            else if (date.Minute % 10 == 0)
            {
                //Parallel.Invoke(() => ConvertToMoreMinuteBar(tickerId, 5));
                Parallel.Invoke(() => ConvertToMoreMinuteBar(tickerId, 10, date));
            }
            else if (date.Minute % 15 == 0)
            {
                //Parallel.Invoke(() => ConvertToMoreMinuteBar(tickerId, 5));
                Parallel.Invoke(() => ConvertToMoreMinuteBar(tickerId, 15, date));
            }
            else if (date.Minute == CloseMinute && date.Hour == CloseHour)
            {
                //Parallel.Invoke(() => { ConvertToFiveMinuteBar(tickerId, 4, date); });
                Parallel.Invoke(() => { ConvertToMoreMinuteBar(tickerId, 60, date); });
                Parallel.Invoke(() => { ConvertToMoreMinuteBar(tickerId, 10, date); });
                Parallel.Invoke(() => { ConvertToMoreMinuteBar(tickerId, 15, date); });
                Parallel.Invoke(() => { ConvertToMoreMinuteBar(tickerId, 30, date); });

                Tf.StartNew((() => { HourAndMoreBars(tickerId, date.Hour); }));
            }
        }

        /// <summary>
        /// Converts to sixty minute bar.
        /// </summary>
        /// <param name="tickerId">The ticker identifier.</param>
        /// <param name="tf">The tf.</param>
        /// <exception cref="ArgumentNullException"></exception>
        private static void ConvertToSixtyMinuteBar(string tickerId, int tf)
        {
            var tickerIdd = tickerId.Substring(0, 2);
            if (tickerIdd!=string.Empty)
            {
                var hourbars = Data.HourBars.FindAll(x => x.Symbol == tickerId).TakeLast(tf / 60).ToList();
                if (hourbars.Count == tf / 60)
                {
                    var bar = ConvertListToBar(hourbars, tickerId, tf);
                    if (bar == null) throw new ArgumentNullException(nameof(bar));

                    Db.AddNewBarInDb(bar);
                }
                else
                {
                    Console.WriteLine("Ticker " + tickerId + "/" + tf + " ma Z€R0 baru!");
                }
            }
        }

        /// <summary>
        ///     Converts the list to bar.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <param name="contractcode"></param>
        /// <param name="tf"></param>
        /// <returns></returns>
        private static MinuteBar ConvertListToBar(List<MinuteBar> list, string contractcode, int tf)
        {
            var barC = list.Last().Close;
            var barH = list.Max(x => x.High);
            var barL = list.Min(x => x.Low);
            var barO = list.First().Open;
            var volume = list.Sum(x => x.Volume);
            var symbol = contractcode;
            var minuteBar = new MinuteBar
            {
                Open = barO,
                High = barH,
                Low = barL,
                Close = barC,
                Interval = tf,
                Symbol = symbol,
                Volume = volume,
                Time = DateTime.Now.AddHours(-7).AddMinutes(-tf)
            };
            Console.WriteLine("New bar O:" + minuteBar.Open / Data.PriceMultiplier + " H:" +
                                           minuteBar.High / Data.PriceMultiplier + " L:" +
                                           minuteBar.Low / Data.PriceMultiplier +
                                           " C:" + minuteBar.Close / Data.PriceMultiplier +
                                           " Symbol:" + minuteBar.Symbol + " Timeframe:" + minuteBar.Interval
                             );
            return minuteBar;
          

        }

        private static void ClearTickList(string tickerid, int date)
        {
            //Data.QuoteList.RemoveAll(x => (x.TickTime.ToString("g") == date.ToString("g")) && (x.TickerId == tickerid));
        }

        private static void ConvertToMinute(int tickerid, DateTime date)
        {
            //var ticks = Data.QuoteList.FindAll(x => (x.TickTime.ToString("g") == date.AddMinutes(-1).ToString("g")) && (x.TickerId == tickerid)).ToList();

            //if (ticks.Count > 0)
            //{
            //    var bar = ConvertListToBar(ticks, tickerid);
            //    Data.ListOhlc.Add(bar);
            //    Db.AddNewBarInDb(bar);
            //}
            //else
            //{
            //    Console.WriteLine("Cubka " + tickerid + " ma malo ticku!" + DateTime.Now.ToString("hh:mm:ss.ffff"));

            //    var bar = Data.ListOhlc.FindLast(x => (x.TickerId == tickerid));
            //    if (bar != null)
            //    {
            //        Data.ListOhlc.Add(bar);
            //        Db.AddNewBarInDb(bar);
            //    }
            //    else
            //    {
            //        Console.WriteLine("No bars found!");
            //    }
            //}
        }

        /// <summary>
        ///     Converts the list to bar.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <param name="tickerid"></param>
        /// <returns></returns>
        private static void ConvertListToBar(List<Quote> list, int tickerid)
        {
            //var barC = list.Last().LastPrice;
            //var barH = list.MaxBy(x => x.LastPrice).LastPrice;
            //var barL = list.MinBy(x => x.LastPrice).LastPrice;
            //var barO = list.First().LastPrice;
            //Console.WriteLine("New bar:" + tickerid);

            //return new MinuteBars(barO, barH, barL, barC, 0, tickerid);
        }
    }
}