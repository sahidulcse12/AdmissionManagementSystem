using System.Net;
using System.Text.Json;
using Admission.Tests.Infrastructure;

namespace Admission.Tests;

/// <summary>Checks the exact JSON field names and status codes required by the assignment contract.</summary>
[Collection(ApiCollection.Name)]
public sealed class ContractTests(ApiFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Hold_endpoints_return_contract_fields_and_status_codes()
    {
        var cls = await _client.CreateClassAsync(seats: 2);
        var student = TestData.StudentId();

        var placed = await _client.PlaceHoldAsync(cls.Id, student);
        Assert.Equal(HttpStatusCode.Created, placed.StatusCode);
        var placedJson = await placed.Content.ReadAsStringAsync();
        ClassApiTests.AssertHasFields(placedJson, "holdId", "classId", "studentId", "status", "expiresAt");
        var holdId = Guid.Parse(JsonDocument.Parse(placedJson).RootElement.GetProperty("holdId").GetString()!);

        var read = await _client.GetHoldAsync(holdId, null);
        Assert.Equal(HttpStatusCode.OK, read.StatusCode);
        ClassApiTests.AssertHasFields(await read.Content.ReadAsStringAsync(), "holdId", "classId", "status", "expiresAt");

        var mine = await _client.MyHoldsAsync(student);
        using (var list = JsonDocument.Parse(await mine.Content.ReadAsStringAsync()))
            ClassApiTests.AssertHasFields(list.RootElement[0].GetRawText(), "holdId", "classId", "className", "status", "expiresAt");

        var confirmed = await _client.ConfirmHoldAsync(holdId, student);
        Assert.Equal(HttpStatusCode.OK, confirmed.StatusCode);
        var confirmedJson = await confirmed.Content.ReadAsStringAsync();
        ClassApiTests.AssertHasFields(confirmedJson, "holdId", "status");
        Assert.Equal("CONFIRMED", JsonDocument.Parse(confirmedJson).RootElement.GetProperty("status").GetString());

        var other = await _client.PlaceHoldOkAsync(cls.Id, TestData.StudentId());
        var deleted = await _client.CancelHoldAsync(other.HoldId, other.StudentId);
        Assert.Equal(HttpStatusCode.NoContent, deleted.StatusCode);
    }

    [Fact]
    public async Task Full_class_returns_409()
    {
        var cls = await _client.CreateClassAsync(seats: 1);
        await _client.PlaceHoldOkAsync(cls.Id, TestData.StudentId());

        var response = await _client.PlaceHoldAsync(cls.Id, TestData.StudentId());

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }
}
