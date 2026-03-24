using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace TekinTeknikServis.Core.Infrastructure
{
    public static class SessionJsonExtensions
    {
        public static void SetJson<T>(this ISession session, string key, T value)
        {
            var json = JsonSerializer.Serialize(value);
            session.SetString(key, json);
        }

        public static T? GetJson<T>(this ISession session, string key)
        {
            var json = session.GetString(key);
            if (string.IsNullOrWhiteSpace(json)) return default;
            return JsonSerializer.Deserialize<T>(json);
        }
    }
}

