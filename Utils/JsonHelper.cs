using System.Text.Json;

namespace DoctorRoutePlanner.Utils
{
    public static class JsonHelper
    {
        public static bool TryParseJson<T>(string json, out T? result)
        {
            result = default;
            try
            {
                result = JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                return result != null;
            }
            catch
            {
                return false;
            }
        }
    }
}