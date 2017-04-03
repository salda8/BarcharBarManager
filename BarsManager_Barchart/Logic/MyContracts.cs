using BarsManager_Barchart.Backend;
using System;
using System.Collections.Generic;
using System.Threading;
using ddfplus.Net;

namespace BarsManager_Barchart.Logic
{
    public class MyContracts
    {
        public static List<string> ContractsList = new List<string>();
        public static List<string> SymbolsList = new List<string>();
        public static List<string> TickerIdList = new List<string>();
        public static string SymbolList = "";

        public static void GetSymbols()
        {
            using (var con = Db.OpenConnection())
            {
                var cmd = con.CreateCommand();
                cmd.CommandText =
                    "SELECT Distinct(Symbol) FROM Contracts_ext";

                using (var reader = cmd.ExecuteReader())

                {
                    while (reader.Read())
                    {
                        if (reader.GetString(0) != null)
                        {
                            SymbolsList.Add(reader.GetString(0));
                            //GetContractsFromDbOnStartup(reader.GetString(0));
                        }
                    }
                }
            }
        }

        public static void GetTickerId(Core core)
        {
            using (var con = Db.OpenConnection())
            {
                var cmd = con.CreateCommand();
                cmd.CommandText =
                    "SELECT Distinct(CC) FROM Contracts";

                using (var reader = cmd.ExecuteReader())

                {
                    while (reader.Read())
                    {
                        TickerIdList.Add(reader.GetString(0));
                        GetContractsFromDbOnStartup(reader.GetString(0), core);
                        Thread.Sleep(1000);
                    }
                }
            }
        }

        /// <summary>
        ///     Gets all contracts.
        /// </summary>
        public static void GetContractsFromDbOnStartup(string symbol, Core core)
        {
            using (var con = Db.OpenConnection())
            {
                using (var cmd = con.CreateCommand())
                {
                    cmd.CommandText =
                        "SELECT ContractCode FROM contracts WHERE CC = ?symbol AND Rollover_Date>?date ORDER BY contracts_id ASC LIMIT 1";
                    cmd.Parameters.AddWithValue("?symbol", symbol);
                    cmd.Parameters.AddWithValue("?date", DateTime.Now.Date);

                    using (var reader = cmd.ExecuteReader())

                    {
                        while (reader.Read())
                        {
                            if (reader.GetString(0) != null)
                            {
                                var contractcode = reader.GetString(0);
                                var requests = "";
                               // requests += "s"; //request a snapshot/refresh quote
                               // requests += "S"; //request quote stream
                                requests += "O"; //ohlc minute bars

                                var requeststring = contractcode + "=" + requests;

                                if (requests.Length > 0)
                                {
                                    if (core.Client.Symbols.Length > 0)
                                    {
                                        //SymbolList += "," + requeststring;
                                        
                                        core.Client.Symbols += "," + requeststring;
                                        SymbolList += "," + requeststring;
                                    }
                                    else
                                    {
                                        //SymbolList += requeststring;
                                        if (core.Status == Status.Disconnected)
                                        {
                                            // Set connection properties prior to connecting
                                            core.Connect();
                                        }
                                        core.Client.Symbols = requeststring;
                                        SymbolList = requeststring;
                                    }
                                }

                                //if (requests.Length > 0)
                                //{
                                //    if (SymbolList.Length > 0)
                                //    {
                                //        SymbolList += "," + requeststring;

                                //    }
                                //    else
                                //    {
                                //        SymbolList += requeststring;

                                //    }
                                //}
                                ContractsList.Add(contractcode);
                                foreach (var tf in Data.TimeframeList)
                                {
                                    HistoryRequest.ExecuteRequest(contractcode, tf, 100);
                                }
                            }

                            //var client = new Core();

                            //Program.Tf.StartNew((() => RemoveDuplicate(symbol)));
                            //Thread.Sleep(100);
                        }
                    }
                }
            }
        }

        public static void GetNewContractsAndData()
        {
            foreach (var tickerid in TickerIdList)
            {
                GetContractsFromDb(tickerid);
            }
        }
        public static void ClearContracts(Core core)
        {
            SymbolList = "";
            core.Client.Symbols = "";
            ContractsList.Clear();
        }
        public static void RemoveDuplicate(string symbol)
        {
            using (var con = Db.OpenConnection())
            {
                using (var cmd = con.CreateCommand())
                {
                    cmd.CommandText =
                        "DELETE FROM minutebars WHERE TimeFrame=1 AND Symbol=?symbol AND idMinuteBars NOT IN (SELECT MIN(idMinuteBars) FROM minutebars GROUP BY bartime);";
                    cmd.Parameters.AddWithValue("?symbol", symbol);
                    var res = cmd.ExecuteNonQuery();
                    if (res > 0)
                    {
                        Console.WriteLine("Duplicate was removed!");
                    }
                    else
                    {
                        Console.WriteLine("No duplicates to remove!");
                    }
                }
            }
        }

        public static void GetContractsFromDb(string tickerId)
        {
            using (var con = Db.OpenConnection())
            {
                var cmd = con.CreateCommand();
                cmd.CommandText =
                    "SELECT ContractCode FROM contracts_ext WHERE tickerId = ?ticker AND Rollover_Date>?date ORDER BY contracts_ext_id ASC LIMIT 1";
                cmd.Parameters.AddWithValue("?ticker", tickerId);
                cmd.Parameters.AddWithValue("?date", DateTime.UtcNow.Date);

                using (var reader = cmd.ExecuteReader())

                {
                    while (reader.Read())
                    {
                        if (reader.GetString(0) != null)
                        {
                            //stop
                            //start
                            var requests = "";
                            requests += "s"; //request a snapshot/refresh quote
                            requests += "S"; //request quote stream
                            requests += "O"; //ohlc minute bars
                            if (requests.Length > 0)
                            {
                                if (SymbolList.Length > 0) SymbolList += ",";
                                else
                                {
                                    SymbolList += reader.GetString(0) + "=" + requests;
                                }
                            }
                        }

                        //Console.WriteLine(contract.Symbol + " | "+contract.TickerId +" | "+ contract.LocalSymbol+ " | "+contract.RolloverDate + " | " +contract.SecType + " | " + contract.Exchange);
                    }
                }
            }
        }
    }
}