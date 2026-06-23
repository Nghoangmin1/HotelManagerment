using System.Text.Json;
using System.Text.Json.Serialization;

namespace HotelManagement.Shared
{
    public static class ChartHelper
    {
        private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public static string SerializeDataForChart(object data)
        {
            return JsonSerializer.Serialize(data, Options);
        }
    }
}
