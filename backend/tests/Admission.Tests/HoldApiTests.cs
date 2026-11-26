using System.Net;
using System.Net.Http.Json;
using Admission.Tests.Infrastructure;

namespace Admission.Tests;

[Collection(ApiCollection.Name)]
public sealed class HoldApiTests(ApiFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Placing_a_hold_reserves_a_seat_for_the_configured_duration()
    {
        var cls = await _client.CreateClassAsync(capacity: 3);
        var student = TestData.StudentId();

        var response = await _client.PlaceHoldAsync(cls.Id, student);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var hold = await response.ReadAsync<HoldDto>();
        Assert.Equal("HELD", hold.Status);
        Assert.Equal(student, hold.StudentId);
        Assert.Equal(cls.Name, hold.ClassName);
        Assert.Equal(TimeSpan.FromSeconds(ApiFactory.HoldDurationSeconds), hold.ExpiresAt - hold.CreatedAt);
        Assert.Equal(ApiFactory.HoldDurationSeconds, hold.RemainingSeconds);
        Assert.Equal($"/holds/{hold.Id}", response.Headers.Location!.OriginalString);

        var after = await _client.GetClassAsync(cls.Id);
        Assert.Equal((1, 0, 2), (after.HeldSeats, after.ConfirmedSeats, after.AvailableSeats));
    }

    [Theory]
    [InlineData(null, "MISSING_STUDENT_ID")]
    [InlineData("", "MISSING_STUDENT_ID")]
    [InlineData("1234", "INVALID_STUDENT_ID")]
    [InlineData("1234567890123456X", "INVALID_STUDENT_ID")]
    [InlineData("123456789012345678", "INVALID_STUDENT_ID")]
    public async Task Student_id_header_is_required_and_validated(string? studentId, string expectedCode)
    {
        var cls = await _client.CreateClassAsync(capacity: 1);

        var response = await _client.PlaceHoldAsync(cls.Id, studentId);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(expectedCode, await response.ErrorCodeAsync());
    }

    [Fact]
    public async Task Hold_on_unknown_class_returns_404()
    {
        var response = await _client.PlaceHoldAsync(Guid.NewGuid(), TestData.StudentId());

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("CLASS_NOT_FOUND", await response.ErrorCodeAsync());
    }

    [Fact]
    public async Task Full_class_rejects_new_holds()
    {
        var cls = await _client.CreateClassAsync(capacity: 1);
        await _client.PlaceHoldOkAsync(cls.Id, TestData.StudentId());

        var response = await _client.PlaceHoldAsync(cls.Id, TestData.StudentId());

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal("NO_SEATS_AVAILABLE", await response.ErrorCodeAsync());
    }

    [Fact]
    public async Task Same_student_cannot_hold_twice_in_one_class()
    {
        var cls = await _client.CreateClassAsync(capacity: 5);
        var student = TestData.StudentId();
        await _client.PlaceHoldOkAsync(cls.Id, student);

        var second = await _client.PlaceHoldAsync(cls.Id, student);

        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
        Assert.Equal("ACTIVE_HOLD_EXISTS", await second.ErrorCodeAsync());
        Assert.Equal(1, (await _client.GetClassAsync(cls.Id)).HeldSeats);
    }

    [Fact]
    public async Task Same_student_may_hold_seats_in_different_classes()
    {
        var a = await _client.CreateClassAsync(capacity: 1);
        var b = await _client.CreateClassAsync(capacity: 1);
        var student = TestData.StudentId();

        await _client.PlaceHoldOkAsync(a.Id, student);
        var second = await _client.PlaceHoldAsync(b.Id, student);

        Assert.Equal(HttpStatusCode.Created, second.StatusCode);
    }

    [Fact]
    public async Task Expired_hold_releases_its_seat_and_lets_the_student_hold_again()
    {
        var cls = await _client.CreateClassAsync(capacity: 1);
        var first = TestData.StudentId();
        var hold = await _client.PlaceHoldOkAsync(cls.Id, first);

        factory.Clock.Advance(TimeSpan.FromSeconds(ApiFactory.HoldDurationSeconds));

        var expired = await (await _client.GetHoldAsync(hold.Id, first)).ReadAsync<HoldDto>();
        Assert.Equal("EXPIRED", expired.Status);
        Assert.Equal(0, expired.RemainingSeconds);
        Assert.Equal(1, (await _client.GetClassAsync(cls.Id)).AvailableSeats);

        // The same student can try again: their stale hold must not block them.
        var again = await _client.PlaceHoldAsync(cls.Id, first);
        Assert.Equal(HttpStatusCode.Created, again.StatusCode);

        // And now the class is full again.
        var other = await _client.PlaceHoldAsync(cls.Id, TestData.StudentId());
        Assert.Equal("NO_SEATS_AVAILABLE", await other.ErrorCodeAsync());
    }

    [Fact]
    public async Task Hold_is_still_active_one_second_before_expiry()
    {
        var cls = await _client.CreateClassAsync(capacity: 1);
        var student = TestData.StudentId();
        var hold = await _client.PlaceHoldOkAsync(cls.Id, student);

        factory.Clock.Advance(TimeSpan.FromSeconds(ApiFactory.HoldDurationSeconds - 1));

        var current = await (await _client.GetHoldAsync(hold.Id, student)).ReadAsync<HoldDto>();
        Assert.Equal("HELD", current.Status);
        Assert.Equal(1, current.RemainingSeconds);
        Assert.Equal("NO_SEATS_AVAILABLE",
            await (await _client.PlaceHoldAsync(cls.Id, TestData.StudentId())).ErrorCodeAsync());
    }

    [Fact]
    public async Task Only_the_owner_can_read_a_hold()
    {
        var cls = await _client.CreateClassAsync(capacity: 1);
        var owner = TestData.StudentId();
        var hold = await _client.PlaceHoldOkAsync(cls.Id, owner);

        var stranger = await _client.GetHoldAsync(hold.Id, TestData.StudentId());
        Assert.Equal(HttpStatusCode.Forbidden, stranger.StatusCode);
        Assert.Equal("HOLD_NOT_OWNED", await stranger.ErrorCodeAsync());

        var missingHeader = await _client.GetHoldAsync(hold.Id, null);
        Assert.Equal(HttpStatusCode.BadRequest, missingHeader.StatusCode);

        var unknown = await _client.GetHoldAsync(Guid.NewGuid(), owner);
        Assert.Equal(HttpStatusCode.NotFound, unknown.StatusCode);

        Assert.Equal(HttpStatusCode.OK, (await _client.GetHoldAsync(hold.Id, owner)).StatusCode);
    }

    [Fact]
    public async Task My_holds_lists_only_the_callers_holds_newest_first()
    {
        var a = await _client.CreateClassAsync(capacity: 2);
        var b = await _client.CreateClassAsync(capacity: 2);
        var me = TestData.StudentId();
        var first = await _client.PlaceHoldOkAsync(a.Id, me);
        factory.Clock.Advance(TimeSpan.FromSeconds(1));
        var second = await _client.PlaceHoldOkAsync(b.Id, me);
        await _client.PlaceHoldOkAsync(a.Id, TestData.StudentId());

        var mine = await (await _client.MyHoldsAsync(me)).ReadAsync<List<HoldDto>>();

        Assert.Equal([second.Id, first.Id], mine.Select(h => h.Id));
    }

    [Fact]
    public async Task Admin_can_list_holds_of_a_class()
    {
        var cls = await _client.CreateClassAsync(capacity: 3);
        await _client.PlaceHoldOkAsync(cls.Id, TestData.StudentId());
        await _client.PlaceHoldOkAsync(cls.Id, TestData.StudentId());

        var holds = await _client.GetFromJsonAsync<List<HoldDto>>($"/classes/{cls.Id}/holds", TestData.Json);

        Assert.Equal(2, holds!.Count);
        Assert.All(holds, h => Assert.Equal(cls.Id, h.ClassId));
    }
}
