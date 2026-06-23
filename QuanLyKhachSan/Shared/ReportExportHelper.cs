using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace HotelManagement.Shared
{
    public static class ReportExportHelper
    {
        public static byte[] ExportToCsv<T>(IEnumerable<T> data)
        {
            var builder = new StringBuilder();
            var properties = typeof(T).GetProperties();
            
            // Add Header
            var headers = properties.Select(p => GetDisplayName(p)).ToArray();
            builder.AppendLine(string.Join(",", headers));
            
            // Add Data
            foreach (var item in data)
            {
                var values = properties.Select(p => 
                {
                    var val = p.GetValue(item, null);
                    return FormatForCsv(val);
                }).ToArray();
                builder.AppendLine(string.Join(",", values));
            }
            
            // UTF-8 with BOM
            byte[] bytes = Encoding.UTF8.GetBytes(builder.ToString());
            byte[] bom = Encoding.UTF8.GetPreamble();
            byte[] result = new byte[bom.Length + bytes.Length];
            Buffer.BlockCopy(bom, 0, result, 0, bom.Length);
            Buffer.BlockCopy(bytes, 0, result, bom.Length, bytes.Length);
            return result;
        }

        private static string GetDisplayName(PropertyInfo property)
        {
            var attribute = property.GetCustomAttribute<DisplayNameAttribute>();
            return attribute != null ? attribute.DisplayName : property.Name;
        }

        private static string FormatForCsv(object? value)
        {
            if (value == null) return string.Empty;
            string strValue = value.ToString() ?? string.Empty;
            
            // Handle quotes and commas for CSV
            if (strValue.Contains(",") || strValue.Contains("\"") || strValue.Contains("\n"))
            {
                strValue = "\"" + strValue.Replace("\"", "\"\"") + "\"";
            }
            return strValue;
        }
    }
}
