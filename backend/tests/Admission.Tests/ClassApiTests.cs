using System.Net;
using System.Net.Http.Json;
using Admission.Tests.Infrastructure;

namespace Admission.Tests;

[Collection(ApiCollection.Name)]
public sealed class ClassApiTests(ApiFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Create_class_returns_201_with_location()
    {
        var name = TestData.UniqueName();
        var response = await _client.PostAsJsonAsync("/classes", new { name, shift = "morning", capacity = 30 });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.ReadAsync<ClassDto>();
        Assert.Equal(name, created.Name);
        Assert.Equal("MORNING", created.Shift);
        Assert.Equal(30, created.Capacity);
        Assert.Equal($"/classes/{created.Id}", response.Headers.Location!.OriginalString);
    }

    [Theory]
    [InlineData(null, "MORNING", 10)]
    [InlineData("  ", "MORNING", 10)]
    [InlineData("Class One", "NIGHT", 10)]
    [InlineData("Class One", null, 10)]
    [InlineData("Class One", "DAY", 0)]
    [InlineData("Class One", "DAY", 1001)]
    [InlineData("Class One", "DAY", null)]
    public async Task Create_class_rejects_invalid_input(string? name, string? shift, int? capacity)
    {
        var response = await _client.PostAsJsonAsync("/classes", new { name, shift, capacity });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("VALIDATION_ERROR", await response.ErrorCodeAsync());
    }

    [Fact]
    public async Task Create_class_rejects_malformed_json()
    {
        var response = await _client.PostAsync("/classes",
            new StringContent("{not json", System.Text.Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("INVALID_REQUEST", await response.ErrorCodeAsync());
    }

    [Fact]
    public async Task Duplicate_name_in_same_shift_is_rejected_case_insensitively()
    {
        var name = TestData.UniqueName();
        (await _client.PostAsJsonAsync("/classes", new { name, shift = "DAY", capacity = 5 })).EnsureSuccessStatusCode();

        var duplicate = await _client.PostAsJsonAsync("/classes", new { name = name.ToUpperInvariant(), shift = "DAY", capacity = 5 });
        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);
        Assert.Equal("CLASS_ALREADY_EXISTS", await duplicate.ErrorCodeAsync());

        var otherShift = await _client.PostAsJsonAsync("/classes", new { name, shift = "MORNING", capacity = 5 });
        Assert.Equal(HttpStatusCode.Created, otherShift.StatusCode);
    }

    [Fact]
    public async Task List_and_get_return_created_class()
    {
        var name = TestData.UniqueName();
        var created = await (await _client.PostAsJsonAsync("/classes", new { name, shift = "EVENING", capacity = 12 }))
            .ReadAsync<ClassDto>();

        var list = await _client.GetFromJsonAsync<List<ClassDto>>("/classes", TestData.Json);
        Assert.Contains(list!, c => c.Id == created.Id && c.Capacity == 12);

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
}
