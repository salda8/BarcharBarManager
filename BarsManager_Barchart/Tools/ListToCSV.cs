using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BarsManager_Barchart.Tools
{
    internal class ListToCsv
    {
        public static string CreateCsvTextFile<T>(List<T> data)
        {
            var properties = typeof (T).GetProperties();
            var result = new StringBuilder();

            foreach (var row in data)
            {
                var values = properties.Select(p => p.GetValue(row, null))
                    .Select(v => StringToCsvCell(Convert.ToString(v)));
                var line = string.Join(",", values);
                result.AppendLine(line);
            }

            return result.ToString();
        }

        private static string StringToCsvCell(string str)
        {
            var mustQuote = (str.Contains(",") || str.Contains("\"") || str.Contains("\r") || str.Contains("\n"));
            if (mustQuote)
            {
                var sb = new StringBuilder();
                sb.Append("\"");
                foreach (var nextChar in str)
                {
                    sb.Append(nextChar);
                    if (nextChar == '"')
                        sb.Append("\"");
                }
                sb.Append("\"");
                return sb.ToString();
            }

            return str;
        }
    }
}