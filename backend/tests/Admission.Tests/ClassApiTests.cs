using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Admission.Tests.Infrastructure;

namespace Admission.Tests;

[Collection(ApiCollection.Name)]
public sealed class ClassApiTests(ApiFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Create_class_accepts_the_contract_body_and_returns_201()
    {
        var name = TestData.UniqueName("Class 6 - Morning Shift");
        var response = await _client.PostAsJsonAsync("/classes", new { name, totalSeats = 120 });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.ReadAsync<ClassDto>();
        Assert.Equal(name, created.Name);
        Assert.Equal(120, created.TotalSeats);
        Assert.Equal($"/classes/{created.Id}", response.Headers.Location!.OriginalString);
    }

    [Fact]
    public async Task Class_responses_use_the_contract_field_names()
    {
        var created = await (await _client.PostAsJsonAsync("/classes",
            new { name = TestData.UniqueName(), totalSeats = 3 })).Content.ReadAsStringAsync();
        AssertHasFields(created, "id", "name", "totalSeats");

        var id = JsonDocument.Parse(created).RootElement.GetProperty("id").GetString();
        var single = await _client.GetStringAsync($"/classes/{id}");
        AssertHasFields(single, "id", "name", "totalSeats", "heldSeats", "confirmedSeats", "availableSeats");

        using var list = JsonDocument.Parse(await _client.GetStringAsync("/classes"));
        Assert.Equal(JsonValueKind.Array, list.RootElement.ValueKind);
        AssertHasFields(list.RootElement[0].GetRawText(), "id", "name", "totalSeats", "heldSeats", "confirmedSeats", "availableSeats");
    }

    [Theory]
    [InlineData(null, 10)]
    [InlineData("  ", 10)]
    [InlineData("Class One", 0)]
    [InlineData("Class One", -5)]
    [InlineData("Class One", 10001)]
    [InlineData("Class One", null)]
    public async Task Create_class_rejects_invalid_input(string? name, int? totalSeats)
    {
        var response = await _client.PostAsJsonAsync("/classes", new { name, totalSeats });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("VALIDATION_ERROR", await response.ErrorCodeAsync());
    }

    [Theory]
    [InlineData("{not json")]
    [InlineData("""{"name":"X","totalSeats":"many"}""")]
    [InlineData("""{"name":"X","totalSeats":1.5}""")]
    [InlineData("")]
    public async Task Malformed_bodies_return_400_not_500(string body)
    {
        var response = await _client.PostAsync("/classes",
            new StringContent(body, System.Text.Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("INVALID_REQUEST", await response.ErrorCodeAsync());
    }

    [Fact]
    public async Task Duplicate_name_is_rejected_case_insensitively_but_each_shift_is_a_separate_class()
    {
        var name = TestData.UniqueName("Class 6");
        (await _client.PostAsJsonAsync("/classes", new { name = $"{name} - Morning Shift", totalSeats = 5 })).EnsureSuccessStatusCode();

        var duplicate = await _client.PostAsJsonAsync("/classes", new { name = $"{name} - MORNING SHIFT", totalSeats = 5 });
        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);
        Assert.Equal("CLASS_ALREADY_EXISTS", await duplicate.ErrorCodeAsync());

        var dayShift = await _client.PostAsJsonAsync("/classes", new { name = $"{name} - Day Shift", totalSeats = 5 });
        Assert.Equal(HttpStatusCode.Created, dayShift.StatusCode);
    }

    [Fact]
    public async Task List_and_get_return_created_class()
    {
        var name = TestData.UniqueName();
        var created = await (await _client.PostAsJsonAsync("/classes", new { name, totalSeats = 12 })).ReadAsync<ClassDto>();

        var list = await _client.GetFromJsonAsync<List<ClassDto>>("/classes", TestData.Json);
        Assert.Contains(list!, c => c.Id == created.Id && c.TotalSeats == 12 && c.AvailableSeats == 12);

        var single = await _client.GetFromJsonAsync<ClassDto>($"/classes/{created.Id}", TestData.Json);
        Assert.Equal(name, single!.Name);
    }

    [Fact]
    public async Task Get_unknown_class_returns_404_and_malformed_id_returns_400()
    {
        var missing = await _client.GetAsync($"/classes/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
        Assert.Equal("CLASS_NOT_FOUND", await missing.ErrorCodeAsync());

        var malformed = await _client.GetAsync("/classes/not-a-guid");
        Assert.Equal(HttpStatusCode.BadRequest, malformed.StatusCode);
        Assert.Equal("INVALID_ID", await malformed.ErrorCodeAsync());
    }

    internal static void AssertHasFields(string json, params string[] fields)
    {
        using var doc = JsonDocument.Parse(json);
        foreach (var field in fields)
            Assert.True(doc.RootElement.TryGetProperty(field, out _), $"Missing field '{field}' in {json}");
    }
}
