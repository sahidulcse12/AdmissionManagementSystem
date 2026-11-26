using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Admission.Tests.Infrastructure;

public static class TestData
{
    public static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    public static string UniqueName(string prefix = "Class") => $"{prefix}-{Guid.NewGuid():N}"[..30];

    public static string StudentId()
    {
        var digits = Random.Shared.NextInt64(10_000_000_000_000_000, 99_999_999_999_999_999);
        return digits.ToString();
    }

    public static async Task<T> ReadAsync<T>(this HttpResponseMessage response) =>
        (await response.Content.ReadFromJsonAsync<T>(Json))!;

    public static async Task<string> ErrorCodeAsync(this HttpResponseMessage response)
    {
        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return doc.RootElement.GetProperty("error").GetProperty("code").GetString()!;
    }
}
