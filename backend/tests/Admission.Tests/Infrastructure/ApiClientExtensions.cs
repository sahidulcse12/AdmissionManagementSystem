using System.Net.Http.Json;

namespace Admission.Tests.Infrastructure;

public static class ApiClientExtensions
{
    public const string StudentHeader = "X-Student-Id";

    public static async Task<ClassDto> CreateClassAsync(this HttpClient client, int seats)
    {
        var response = await client.PostAsJsonAsync("/classes",
            new { name = TestData.UniqueName(), totalSeats = seats });
        response.EnsureSuccessStatusCode();
        return await response.ReadAsync<ClassDto>();
    }

    public static Task<ClassDto> GetClassAsync(this HttpClient client, Guid classId) =>
        client.GetFromJsonAsync<ClassDto>($"/classes/{classId}", TestData.Json)!;

    public static Task<HttpResponseMessage> PlaceHoldAsync(this HttpClient client, Guid classId, string? studentId) =>
        client.SendAsync(Request(HttpMethod.Post, $"/classes/{classId}/holds", studentId));

    public static Task<HttpResponseMessage> GetHoldAsync(this HttpClient client, Guid holdId, string? studentId) =>
        client.SendAsync(Request(HttpMethod.Get, $"/holds/{holdId}", studentId));

    public static Task<HttpResponseMessage> ConfirmHoldAsync(this HttpClient client, Guid holdId, string? studentId) =>
        client.SendAsync(Request(HttpMethod.Post, $"/holds/{holdId}/confirm", studentId));

    public static Task<HttpResponseMessage> CancelHoldAsync(this HttpClient client, Guid holdId, string? studentId) =>
        client.SendAsync(Request(HttpMethod.Delete, $"/holds/{holdId}", studentId));

    public static Task<HttpResponseMessage> MyHoldsAsync(this HttpClient client, string? studentId) =>
        client.SendAsync(Request(HttpMethod.Get, "/me/holds", studentId));

    public static async Task<HoldDto> PlaceHoldOkAsync(this HttpClient client, Guid classId, string studentId)
    {
        var response = await client.PlaceHoldAsync(classId, studentId);
        response.EnsureSuccessStatusCode();
        return await response.ReadAsync<HoldDto>();
    }

    private static HttpRequestMessage Request(HttpMethod method, string url, string? studentId)
    {
        var request = new HttpRequestMessage(method, url);
        if (studentId is not null) request.Headers.Add(StudentHeader, studentId);
        return request;
    }
}
