using System;
using System.Collections.Generic;
using System.Threading;
using BarsManager_Barchart.Logic;
using BarsManager_Barchart.Types;
using ddfplus;
using ddfplus.Historical.Client;

namespace BarsManager_Barchart.Backend
{
    public class HistoryRequest
    {
        private const string Server = "";
        private const string Interval = "Minute";
        //private static readonly string minute = "1";
        private const bool Async = false;
        private const string Fileformat = "Csv";
        private const bool Gzip = false;
        private static readonly bool Compression = false;
        //private static readonly int maxrecords = 1000;
        private static bool cancelRequest;
        private static int totalRecords;
        private static DateTime requestStartTime;
        private static DateTime requestResponseTime;
        private static DateTime responseTransmissionTime;
        private static DateTime responseCompletionTime;

        private string sortorder = "Ascending";

        private static Request AssembleRequest(string symbol, int minutes, int maxrecords)
        {
            Request request = null;
            //if (minutes == 5)
            //{
            //    maxrecords = 100;
            //}
            request = new MinuteRequest();

            if (string.IsNullOrEmpty(Server))
            {
                // Find out who the server is through the Connection object
                Connection.Username = Core.Username;
                Connection.Password = Core.Password;

                Connection.RefreshUserInfo();
            }
            else
            {
                request.Username = Core.Username;
                request.Password = Core.Password;
                request.Server = Server;
            }

            request.Symbol = symbol;
            ((MinuteRequest) request).Interval = (short) minutes;

            request.End = DateTime.Now.AddHours(-7).AddMinutes(-minutes);


            request.DownloadFormat = (DownloadFormat) Enum.Parse(typeof (DownloadFormat), Fileformat);

            request.ServerBufferSize = 0;
            request.GZip = Gzip;
            request.Deflate = Compression;

            request.MaxRecords = maxrecords;
            

            request.SortOrder =
                (SortOrder) Enum.Parse(
                    typeof (SortOrder), "Ascending");

            request.Properties["normalizetimestamps"] = true;

            if (Async)
            {
                var batchSize = 0;
                request.BatchSize = batchSize;
                if (request.BatchSize > 0)
                {
                    request.BatchUnits =
                        (BatchUnits) Enum.Parse(typeof (BatchUnits), "Minutes");
                }
            }

            return request;
        }

        public static void ExecuteRequest(string symbol, int minute, int maxrecords)
        {
            try
            {
                totalRecords = 0;

                var request = AssembleRequest(symbol, minute, maxrecords);

                if (request != null)
                {
                    cancelRequest = false;

                    if (Async)
                    {
                        request.StatusChange += OnStatusChange;
                        if (request is MinuteRequest)
                            request.NewRecord += OnNewMinute;
                    }
                    else
                    {
                      
                        var records = request.GetResponse();
                    
                     

                        if (cancelRequest == false)
                        {
                            AddMinutesToGrid(records, (MinuteRequest) request);
                        }
                    }
                }
            }
            catch (HistoricalServerException hsex)
            {
                Console.WriteLine("Error reported by historical server.\n" + hsex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error executing request: " + ex.Message +
                                  (ex.InnerException != null ? "\n- " + ex.InnerException.Message : ""));
            }
        }

        private static void AddMinutesToGrid(List<IRecord> minutes, MinuteRequest request)
        {
            string symbol = request.Symbol;


            foreach (IRecord rec in minutes)
            {
                if (cancelRequest)
                    break;

                var minute = (IMinute) rec;

                var record = minute as IIntradayRecord;
                var sym = record != null ? record.Symbol : symbol;
                var bar = new MinuteBar
                {
                    Close = (decimal) (minute.Close * Data.PriceMultiplier),
                    High = (decimal) (minute.High * Data.PriceMultiplier),
                    Interval = request.Interval,
                    Low = (decimal) (minute.Low * Data.PriceMultiplier),
                    Open = (decimal) (minute.Open * Data.PriceMultiplier),
                    Symbol = sym,
                    Volume = minute.Volume,
                    Time = minute.Timestamp
                };


                Data.MinuteBars.Add(bar);
                

            }
        }

        private static void OnNewMinute(object sender, NewRecordEventArgs e)
        {
            totalRecords += e.Records?.Count ?? 0;
            var request = sender as Request;
            var symbol = request != null ? request.Symbol : "";

            // AddMinutesToGrid(e.Records, symbol);
        }

        private static void OnStatusChange(object sender, StatusChangeEventArgs e)
        {
            var requestComplete = false;

           Console.WriteLine($"Request status changed from '{e.PreviousState}' to '{e.CurrentState}'\n", true);

            switch (e.CurrentState)
            {
                case RequestStatus.Executing:
                    totalRecords = 0;
                    requestStartTime = DateTime.Now;
                    break;
                case RequestStatus.Receiving:
                    requestResponseTime = DateTime.Now;
                    break;
                case RequestStatus.ResponseComplete:
                    responseTransmissionTime = DateTime.Now;
                    break;
                case RequestStatus.Successful:
                    responseCompletionTime = DateTime.Now;
                    
                    Console.WriteLine(
                        $"{totalRecords} record(s) processed in {responseCompletionTime.Subtract(requestStartTime)} total elapsed time\n",
                        true);
                    Console.WriteLine($"- Response time:     {requestResponseTime.Subtract(requestStartTime)}\n", true);
                    Console.WriteLine($"- Transmission time: {responseTransmissionTime.Subtract(requestResponseTime)}\n",
                        true);

                    requestComplete = true;
                    break;
                case RequestStatus.Canceled:
                    Console.WriteLine("Request cancelled by user.\n", true);
                    requestComplete = true;
                    break;
                case RequestStatus.Error:
                    var request = sender as Request;
                    var errorMessage = request != null ? request.ErrorMessage : "unknown error";
                    Console.WriteLine("Error executing request.\n- " + errorMessage + "\n", true);
                    requestComplete = true;
                    break;
            }

            if (requestComplete)
            {
                Console.WriteLine("Request completed!");
            }
        }
    }
}