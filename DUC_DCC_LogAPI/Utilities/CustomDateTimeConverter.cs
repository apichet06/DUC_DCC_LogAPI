using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DUC_DCC_LogAPI.Utilities
{
    public class CustomDateTimeConverter : JsonConverter<DateTime?>
    {
        private const string Format = "yyyy-MM-dd HH:mm:ss";

        public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }

            var dateString = reader.GetString();
            if (string.IsNullOrWhiteSpace(dateString))
            {
                return null;
            }

            if (DateTime.TryParseExact(dateString, Format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            {
                return date;
            }

            // Return null if parsing fails to avoid crashing the application
            return null;
        }
        public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value?.ToString(Format, CultureInfo.InvariantCulture));
        }
    }
}
