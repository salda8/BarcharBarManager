using System;
using System.Text;
using ddfplus;

namespace BarsManager_Barchart.Tools
{
    internal class DdfSerialization
    {
        public static string QuoteToString(Quote q)
        {
            var res = "";

            try
            {
                //var quote =
                //    $"Symbol={q.Symbol}\nUpdate Source={QuoteUpdateSource(q)}\nTimestamp={q.Timestamp.ToString("MM/dd/yyyy HH:mm:ss.fff")}" +
                //    $"\nAsk={q.FormatValue(q.Ask, NumericFormat.Default)}\nAskSize={q.AskSize}\nBid={q.FormatValue(q.Bid, NumericFormat.Default)}" +
                //    $"\nBidSize={q.FormatValue(q.BidSize, NumericFormat.Default)}\nChange={q.FormatValue(q.Change, NumericFormat.Default)}" +
                //    $"\nExchange={(q.Exchange != '\0' ? q.Exchange.ToString() : "")}\nPermission={q.Permission}\nRecord={(q.Record != '\0' ? q.Record.ToString() : "")}" +
                //    $"\nSubrecord={(q.Subrecord != '\0' ? q.Subrecord.ToString() : "")}\nElement={(q.Element != '\0' ? q.Element.ToString() : "")}" +
                //    $"\nModifier={(q.Modifier != '\0' ? q.Modifier.ToString() : "")}\n-Source:{Encoding.ASCII.GetString(q.Source)}\n";
                var quote = $"Symbol={q.Symbol}\nTimestamp={q.Timestamp:MM/dd/yyyy HH:mm:ss.fff}" +
                            $"\nAsk={q.FormatValue(q.Ask, NumericFormat.Default)}\nAskSize={q.AskSize}\nBid={q.FormatValue(q.Bid, NumericFormat.Default)}" +
                            $"\nBidSize={q.FormatValue(q.BidSize, NumericFormat.Default)}\nChange={q.FormatValue(q.Change, NumericFormat.Default)}" +
                            $"\nExchange={(q.Exchange != '\0' ? q.Exchange.ToString() : "")}\nPermission={q.Permission}";

              
                res = quote;
            }
            catch (Exception ex)
            {
                res = "Error converting data from quote to string: " + ex.Message;
            }

            return res;
        }

        public static string SessionToString(Session s, char unitcode)
        {
            var res = "";

            try
            {
                if (s != null)
                {
                    res =
                        $"Timestamp={s.Timestamp:MM/dd/yyyy HH:mm:ss.fff}\nLast={Quote.FormatValue(s.Last, NumericFormat.Default, unitcode)}\nLastSize={s.LastSize}\nOpen1={Quote.FormatValue(s.Open1, NumericFormat.Default, unitcode)}\nOpen2={Quote.FormatValue(s.Open2, NumericFormat.Default, unitcode)}\nHigh={Quote.FormatValue(s.High, NumericFormat.Default, unitcode)}\nLow={Quote.FormatValue(s.Low, NumericFormat.Default, unitcode)}\nClose1={Quote.FormatValue(s.Close1, NumericFormat.Default, unitcode)}\nClose2={Quote.FormatValue(s.Close2, NumericFormat.Default, unitcode)}\nVolume={s.Volume}\nOpenInterest={s.OpenInterest}\nTickTrend={s.TickTrend}\nTradeSession={s.TradeSession}\nDay={s.Day}\n";
                }
            }
            catch (Exception ex)
            {
                res = "Error converting data from session to string: " + ex.Message;
            }

            return res;
        }

        public static string BookQuoteToString(BookQuote q)
        {
            var res = "";

            try
            {
                var askCount = Math.Min(q.AskPrices.Length, q.AskPrices.Length);
                var askItems = "";
                for (var i = 0; i < askCount; i++)
                {
                    if (askItems.Length > 0)
                        askItems += ",";
                    askItems += $"{q.FormatValue(q.AskPrices[i], NumericFormat.Default)}x{q.AskSizes[i]}";
                }

                var bidCount = Math.Min(q.BidPrices.Length, q.BidPrices.Length);
                var bidItems = "";
                for (var i = 0; i < bidCount; i++)
                {
                    if (bidItems.Length > 0)
                        bidItems += ",";
                    bidItems += $"{q.FormatValue(q.BidPrices[i], NumericFormat.Default)}x{q.BidSizes[i]}";
                }

                res =
                    $"Symbol={q.Symbol}\nTimestamp={q.Timestamp:yyyy-MM-dd HH:mm:ss}\nAsks={askItems}\nBids={bidItems}\nSource={Encoding.ASCII.GetString(q.Source)}\n";
            }
            catch (Exception ex)
            {
                res = "Error converting data from book quote to string: " + ex.Message;
            }

            return res;
        }

        public static string OhlcQuoteToString(OHLCQuote q)
        {
            var res = "";

            try
            {
                res =
                    $"{q.Symbol} @ {q.Timestamp:yyyy-MM-dd HH:mm}: Day={q.Day},Open={q.FormatValue(q.Open, NumericFormat.Default)},High={q.FormatValue(q.High, NumericFormat.Default)},Low={q.FormatValue(q.Low, NumericFormat.Default)},Close={q.FormatValue(q.Close, NumericFormat.Default)},Volume={q.Volume}";
            }
            catch (Exception ex)
            {
                res = "Error converting data from OHLC quote to string: " + ex.Message;
            }

            return res;
        }

        public static string QuoteUpdateSource(Quote q)
        {
            var source = "";

            if (q.IsRefresh && q.IsTrade)
                source = "trade/refresh message";
            else if (q.IsRefresh)
                source = "refresh message";
            else if (q.IsTrade)
                source = "trade message";
            else if ((q.Record == '2') && (q.Subrecord == '8'))
                source = "bid/ask message";
            else
                source = $"{q.Record}.{q.Subrecord} type message";

            return source;
        }
    }
}