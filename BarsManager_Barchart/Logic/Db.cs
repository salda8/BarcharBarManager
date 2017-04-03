using System;
using System.Data.SqlClient;
using BarsManager_Barchart.Types;
using ddfplus;

namespace BarsManager_Barchart.Logic
{
    public class Db
    {
        public static string ConnectionString = Properties.Settings.Default.SqlConnectionsstring;

        /// <summary>
        ///     Opens the connection.
        /// </summary>
        /// <returns></returns>
        public static SqlConnection OpenConnection()
        {
            var connection = new SqlConnection(ConnectionString);
            try
            {
                connection.Open();
            }
            catch (SqlException exception)
            {
                Console.WriteLine(exception.Message);
            }

            return connection;
        }

        public static void TruncateMinutebarsTable()
        {
            using (var con = OpenConnection())
            {
                using (var command = con.CreateCommand())
                {
                    command.CommandText =
                        "TRUNCATE minutebars";
                    command.ExecuteNonQuery();
                    command.CommandText =
                        "TRUNCATE ticks";
                    command.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        ///     Adds the new bar in database.
        /// </summary>
        /// <param name="minuteBar">The minute bar.</param>
        public static
            void AddNewBarInDb(MinuteBar minuteBar)
        {
            using (var con = OpenConnection())
            {
                using (var command = con.CreateCommand())
                {
                    command.CommandText =
                        "INSERT INTO minutebars (open, high, low, close, symbol, TimeFrame,volume, bartime) VALUES (?o,?h,?l,?c,?s,?tf,?v, ?bt)";
                    command.Parameters.AddWithValue("?o",(minuteBar.Open/ Data.PriceMultiplier));
                    command.Parameters.AddWithValue("?h", minuteBar.High / Data.PriceMultiplier);
                    command.Parameters.AddWithValue("?l", minuteBar.Low / Data.PriceMultiplier);
                    command.Parameters.AddWithValue("?c", minuteBar.Close / Data.PriceMultiplier);
                    command.Parameters.AddWithValue("?s", minuteBar.Symbol.Substring(0, 2));
                    command.Parameters.AddWithValue("?v", minuteBar.Volume);
                    command.Parameters.AddWithValue("?tf", minuteBar.Interval);
                    command.Parameters.AddWithValue("?bt", minuteBar.Time);

                    var result = command.ExecuteNonQuery();
                    if (result == 1)
                    {
                        Console.WriteLine("INB in DB:" + minuteBar.Open / Data.PriceMultiplier + " H:" +
                                          minuteBar.High / Data.PriceMultiplier + " L:" +
                                          minuteBar.Low / Data.PriceMultiplier +
                                          " C:" + minuteBar.Close / Data.PriceMultiplier +
                                          " Symbol:" + minuteBar.Symbol + " Timeframe:" + minuteBar.Interval
                            );
                    }
                    else
                    {
                        Console.WriteLine("Adding new bar into db was unsuccessful!");
                    }
                }
            }
        }

        /// <summary>
        ///     Adds the new bar in database.
        /// </summary>
        /// <param name="tick">The tick.</param>
        public static void AddNewQuoteInDb(Quote tick)
        {
            using (var con = OpenConnection())
            {
                using (var command = con.CreateCommand())
                {
                    command.CommandText =
                        "INSERT INTO ticks (Price, Symbol, Time) VALUES (?o,?s,?t)";
                    command.Parameters.AddWithValue("?o", tick.Ask);

                    command.Parameters.AddWithValue("?s", tick.Symbol.Substring(0,2));

                    command.Parameters.AddWithValue("?t", tick.Timestamp);


                    var result = command.ExecuteNonQuery();
                    if (result > 0)
                    {
                        //Console.WriteLine("New RTBar O:" + tick.Open + " H:" + tick.High + " L:" + tick.Low + " C:" + tick.Close +
                        //    " Symbol:" + tick.Symbol + " Time:" + tick.BarDateTime + " Volume:" + tick.Volume);
                    }
                    else
                    {
                        Console.WriteLine("Adding new bar into db was unsuccessful!");
                    }
                }
            }
        }

        public static void AddTestTimeInDb(TestCore testList)
        {
            using (var con = OpenConnection())
            {
                using (var command = con.CreateCommand())
                {
                    command.CommandText =
                        "INSERT INTO testcore ( Symbol, Time, Number) VALUES (?s,?t,?n)";
                    command.Parameters.AddWithValue("?s", testList.Symbol);

                    command.Parameters.AddWithValue("?t", testList.Time);
                    command.Parameters.AddWithValue("?n", testList.Number);


                    var result = command.ExecuteNonQuery();
                    if (result > 0)
                    {
                        //Console.WriteLine("New RTBar O:" + tick.Open + " H:" + tick.High + " L:" + tick.Low + " C:" + tick.Close +
                        //    " Symbol:" + tick.Symbol + " Time:" + tick.BarDateTime + " Volume:" + tick.Volume);
                    }
                    else
                    {
                        Console.WriteLine("Adding new bar into db was unsuccessful!");
                    }
                }
            }
        }

        public static void AddOhlcQuoteInDb(OHLCQuote minuteBar)
        {
            using (var con = OpenConnection())
            {
                using (var command = con.CreateCommand())
                {
                    command.CommandText =
                        "INSERT INTO minutebars (open, high, low, close, symbol, TimeFrame, bartime, volume) VALUES (?o,?h,?l,?c,?s,?tf, ?t,?v)";
                    command.Parameters.AddWithValue("?o", minuteBar.Open );
                    command.Parameters.AddWithValue("?h", minuteBar.High );
                    command.Parameters.AddWithValue("?l", minuteBar.Low );
                    command.Parameters.AddWithValue("?c", minuteBar.Close);
                    command.Parameters.AddWithValue("?s", minuteBar.Symbol.Substring(0, 2));
                    command.Parameters.AddWithValue("?v", minuteBar.Volume);
                    command.Parameters.AddWithValue("?tf", minuteBar.Interval);
                    command.Parameters.AddWithValue("?t", minuteBar.Timestamp);


                    var result = command.ExecuteNonQuery();
                    if (result > 0)
                    {
                        Console.WriteLine("New bar O:" + minuteBar.Open + " H:" +
                                          minuteBar.High + " L:" +
                                          minuteBar.Low  +
                                          " C:" + minuteBar.Close +
                                          " Symbol:" + minuteBar.Symbol + " Timeframe:" + minuteBar.Interval
                            );
                    }
                    else
                    {
                        Console.WriteLine("Adding new bar into db was unsuccessful!");
                    }
                }
            }
        }

        public static void AddNewTimestamp(DateTime timestamp, DateTime loctime)
        {
            using (var con = OpenConnection())
            {
                using (var command = con.CreateCommand())
                {
                    command.CommandText =
                        "INSERT INTO servertime(time, loctime) VALUE (?times, ?time)";
                    command.Parameters.AddWithValue("?times", timestamp);
                    command.Parameters.AddWithValue("?time", loctime);


                    var result = command.ExecuteNonQuery();
                    if (result > 0)
                    {
                        //Console.WriteLine(timestamp.ToString("hh:mm:ss.ffff") + " vs. " +
                        //      loctime.ToString("hh:mm:ss.ffff"));
                    }
                    else
                    {
                        Console.WriteLine("Adding new bar into db was unsuccessful!");
                    }
                }
            }
        }
    }
}