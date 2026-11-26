using System.Net;
using System.Text.Json;
using Admission.Tests.Infrastructure;

namespace Admission.Tests;

[Collection(ApiCollection.Name)]
public sealed class ApiVersioningAndSwaggerTests(ApiFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Requests_without_a_version_use_v1_and_report_supported_versions()
    {
        var response = await _client.GetAsync("/classes");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(response.Headers.TryGetValues("api-supported-versions", out var versions));
        Assert.Contains("1.0", string.Join(",", versions!));
    }

    [Theory]
    [InlineData("header")]
    [InlineData("query")]
    public async Task Version_1_can_be_requested_by_header_or_query(string how)
    {
        var request = how == "header"
            ? new HttpRequestMessage(HttpMethod.Get, "/classes") { Headers = { { "X-Api-Version", "1.0" } } }
            : new HttpRequestMessage(HttpMethod.Get, "/classes?api-version=1.0");

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Unsupported_version_is_rejected_with_400()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/classes") { Headers = { { "X-Api-Version", "9.0" } } };

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Swagger_ui_and_document_are_served_with_all_contract_paths()
    {
        var ui = await _client.GetAsync("/swagger/index.html");
        Assert.Equal(HttpStatusCode.OK, ui.StatusCode);

        var doc = await _client.GetAsync("/swagger/v1/swagger.json");
        Assert.Equal(HttpStatusCode.OK, doc.StatusCode);

        using var json = JsonDocument.Parse(await doc.Content.ReadAsStringAsync());
        var paths = json.RootElement.GetProperty("paths").EnumerateObject().Select(p => p.Name).ToHashSet();
        Assert.Superset(paths, new HashSet<string>
        {
            "/classes", "/classes/{classId}", "/classes/{classId}/holds",
            "/holds/{holdId}", "/holds/{holdId}/confirm", "/me/holds",
        });
    }
}
