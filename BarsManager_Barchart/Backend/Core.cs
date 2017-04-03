using System;
using System.Text;
using BarsManager_Barchart.Logic;
using BarsManager_Barchart.Tools;
using BarsManager_Barchart.Types;
using ddfplus;
using ddfplus.Net;
using ddfplus.Parser;

namespace BarsManager_Barchart.Backend
{
    public class Core
    {
        private const string Connectionprotocol = "TCP";
        public static readonly string Username = Properties.Settings.Default.BarchartLogin;
        public static readonly string Password = Properties.Settings.Default.BarchartPassword;
        public static readonly string Server = Properties.Settings.Default.BarchartServer;
        public static readonly int Port = Properties.Settings.Default.Port;
        private const bool AutoResolve = true;
        public static DateTime ServerDateTime;
        private const string Filter = "Filtered"; //"Filtered","Unfiltered","Raw");
        public Client Client;
        public Status Status = Status.Disconnected;

        public Core()
        {
            InitializeDdf();
            UpdateConnectionDiagnostics();
        }

        private void InitializeDdf()
        {
            // The streaming version must be set prior to creating any clients expected to work for that version
            Connection.Properties["streamingversion"] = "3";
            Client = new Client
            {
                Filter = (Filter) Enum.Parse(typeof(Filter), Filter, true)
            };

            Client.NewQuote += _client_NewQuote;
            Client.NewBookQuote += _client_NewBookQuote;
            Client.NewTimestamp += _client_NewTimestamp;
            Client.NewOHLCQuote += _client_NewOHLCQuote;
            Client.NewMessage += _client_NewMessage;
            //_client.Symbols += MyContracts.SymbolList;

            Connection.StatusChange += Connection_StatusChange;
        }

        public void Connect()
        {
            Connection.Password = Password;
            Connection.Mode = Connectionprotocol == "UDP" ? ConnectionMode.UDPClient : ConnectionMode.TCPClient;
            if (!AutoResolve)
            {
                Connection.Server = Server;
                Connection.Port = Port > 0 ? (ushort) Port : (ushort) 0;
            }
            Connection.Server = "";
            Connection.Port = 0;
        }

        private void UpdateConnectionDiagnostics()
        {
            Connection.Properties["traceerrors"] = true;
            Connection.Properties["tracewarnings"] = true;
            Connection.Properties["traceinfo"] = true;
            Connection.Properties["messagetracefilter"] = true;
            Connection.Username = Username;
        }


        private void NewMessage(byte[] message)
        {
            var ddfMessage = MessageParser.ParseMessage(new Stream(message));

            var messageType = "";

            if (ddfMessage is MessageTimestamp)
            {
                messageType = "Timestamp";
            }
            else
            {
                messageType = "ddf Message";
            }

            var messageContents = Encoding.ASCII.GetString(message);
            Console.WriteLine(($"{messageType}: {messageContents}"));
            Data.ListRaw.Add(message);
        }

        private void NewOhlcQuote(OHLCQuote q)
        {
            Data.MinuteBars.Add(new MinuteBar
            {
                Close = (decimal) (q.Close * Data.PriceMultiplier),
                High = (decimal) (q.High * Data.PriceMultiplier),
                Interval = (int) q.Interval,
                Low = (decimal) (q.Low * Data.PriceMultiplier),
                Open = (decimal) (q.Open * Data.PriceMultiplier),
                Symbol = q.Symbol,
                Volume = q.Volume,
                Time = q.Timestamp
            });
            Program.Tf.StartNew(() => Db.AddOhlcQuoteInDb(q));
            //Console.WriteLine("New bar: "+q.Timestamp.ToString("hh:mm:ss.ffff") +" vs. "+ DateTime.Now.AddHours(-7).ToString("hh:mm:ss.ffff"));
        }

        private void NewQuote(Quote quote)
        {
            if (quote.Permission.ToString() == "RealTime" && quote.IsTrade)
            {
                Data.QuoteList.Add(quote);
                Program.Tf.StartNew((() => Db.AddNewQuoteInDb(quote)));
                Console.WriteLine(DdfSerialization.QuoteToString(quote));
            }
            //else
            //{
            //    Console.WriteLine("another delayed bar...");
            //}
        }

        private void NewBookQuote(BookQuote quote)
        {
            Data.BookList.Add(quote);
        }

        private void UpdateConnectionStatus(Status status)
        {
            Console.WriteLine("Connection Status:" + status);
            Status = status;
            if (Status == Status.Disconnected && Program.Variable == 1)
            {
                Program.Tf.StartNew((() => Program.Barchart(this)));
            }
        }

        #region ddf Connection Event Handlers

        private void Connection_StatusChange(object sender, StatusChangeEventArgs e)
        {
            UpdateConnectionStatus(e.NewStatus);
        }

        #endregion ddf Connection Event Handlers

        #region ddf Client Event Handlers

        private void _client_NewMessage(object sender, Client.NewMessageEventArgs e)
        {
            NewMessage(e.Message);
        }

        private void _client_NewOHLCQuote(object sender, Client.NewOHLCQuoteEventArgs e)
        {
            var sym = e.OHLCQuote.Symbol.Substring(0, 2);
            if (sym == "A6" || sym == "D6" || sym == "J6" || sym == "E6" || sym == "B6")
            {
                NewOhlcQuote(e.OHLCQuote);
            }
        }

        private void _client_NewTimestamp(object sender, Client.NewTimestampEventArgs e)
        {
            ServerDateTime = e.Timestamp;

            Program.Tf.StartNew((() => Db.AddNewTimestamp(e.Timestamp, DateTime.Now.AddHours(-7))));
        }

        private void _client_NewBookQuote(object sender, Client.NewBookQuoteEventArgs e)
        {
            NewBookQuote(e.BookQuote);
        }

        private void _client_NewQuote(object sender, Client.NewQuoteEventArgs e)
        {
            NewQuote(e.Quote);
        }

        #endregion ddf Client Event Handlers
    }
}