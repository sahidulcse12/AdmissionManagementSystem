using System.Net;
using Admission.Tests.Infrastructure;

namespace Admission.Tests;

[Collection(ApiCollection.Name)]
public sealed class AdmissionApiTests(ApiFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Confirming_a_hold_admits_the_student()
    {
        var cls = await _client.CreateClassAsync(capacity: 2);
        var student = TestData.StudentId();
        var hold = await _client.PlaceHoldOkAsync(cls.Id, student);

        var response = await _client.ConfirmHoldAsync(hold.Id, student);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var confirmed = await response.ReadAsync<HoldDto>();
        Assert.Equal("CONFIRMED", confirmed.Status);
        Assert.NotNull(confirmed.ConfirmedAt);
        Assert.Equal(0, confirmed.RemainingSeconds);

        var after = await _client.GetClassAsync(cls.Id);
        Assert.Equal((0, 1, 1), (after.HeldSeats, after.ConfirmedSeats, after.AvailableSeats));
    }

    [Fact]
    public async Task Confirmation_is_idempotent()
    {
        var cls = await _client.CreateClassAsync(capacity: 2);
        var student = TestData.StudentId();
        var hold = await _client.PlaceHoldOkAsync(cls.Id, student);

        var first = await (await _client.ConfirmHoldAsync(hold.Id, student)).ReadAsync<HoldDto>();
        factory.Clock.Advance(TimeSpan.FromSeconds(5));
        var secondResponse = await _client.ConfirmHoldAsync(hold.Id, student);
        var second = await secondResponse.ReadAsync<HoldDto>();

        Assert.Equal(HttpStatusCode.OK, secondResponse.StatusCode);
        Assert.Equal(first.ConfirmedAt, second.ConfirmedAt);
        Assert.Equal(1, (await _client.GetClassAsync(cls.Id)).ConfirmedSeats);
    }

    [Fact]
    public async Task Confirmed_admission_stays_valid_after_the_hold_window()
    {
        var cls = await _client.CreateClassAsync(capacity: 1);
        var student = TestData.StudentId();
        var hold = await _client.PlaceHoldOkAsync(cls.Id, student);
        await _client.ConfirmHoldAsync(hold.Id, student);

        factory.Clock.Advance(TimeSpan.FromSeconds(ApiFactory.HoldDurationSeconds * 2));

        var later = await (await _client.GetHoldAsync(hold.Id, student)).ReadAsync<HoldDto>();
        Assert.Equal("CONFIRMED", later.Status);
        Assert.Equal(0, (await _client.GetClassAsync(cls.Id)).AvailableSeats);
        Assert.Equal("ALREADY_ADMITTED",
            await (await _client.PlaceHoldAsync(cls.Id, student)).ErrorCodeAsync());
    }

    [Fact]
    public async Task Expired_hold_cannot_be_confirmed()
    {
        var cls = await _client.CreateClassAsync(capacity: 1);
        var student = TestData.StudentId();
        var hold = await _client.PlaceHoldOkAsync(cls.Id, student);

        factory.Clock.Advance(TimeSpan.FromSeconds(ApiFactory.HoldDurationSeconds));
        var response = await _client.ConfirmHoldAsync(hold.Id, student);

        Assert.Equal(HttpStatusCode.Gone, response.StatusCode);
        Assert.Equal("HOLD_EXPIRED", await response.ErrorCodeAsync());
        Assert.Equal(0, (await _client.GetClassAsync(cls.Id)).ConfirmedSeats);
    }

    [Fact]
    public async Task Cancelled_hold_cannot_be_confirmed()
    {
        var cls = await _client.CreateClassAsync(capacity: 1);
        var student = TestData.StudentId();
        var hold = await _client.PlaceHoldOkAsync(cls.Id, student);
        (await _client.CancelHoldAsync(hold.Id, student)).EnsureSuccessStatusCode();

        var response = await _client.ConfirmHoldAsync(hold.Id, student);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal("HOLD_CANCELLED", await response.ErrorCodeAsync());
    }

    [Fact]
    public async Task Only_the_owner_can_confirm_or_cancel()
    {
        var cls = await _client.CreateClassAsync(capacity: 1);
        var owner = TestData.StudentId();
        var stranger = TestData.StudentId();
        var hold = await _client.PlaceHoldOkAsync(cls.Id, owner);

        var confirm = await _client.ConfirmHoldAsync(hold.Id, stranger);
        var cancel = await _client.CancelHoldAsync(hold.Id, stranger);

        Assert.Equal(HttpStatusCode.Forbidden, confirm.StatusCode);
        Assert.Equal("HOLD_NOT_OWNED", await confirm.ErrorCodeAsync());
        Assert.Equal(HttpStatusCode.Forbidden, cancel.StatusCode);
        var unchanged = await (await _client.GetHoldAsync(hold.Id, owner)).ReadAsync<HoldDto>();
        Assert.Equal("HELD", unchanged.Status);
    }

    [Fact]
    public async Task Confirm_and_cancel_validate_identity_and_ids()
    {
        var missingHeader = await _client.ConfirmHoldAsync(Guid.NewGuid(), null);
        Assert.Equal(HttpStatusCode.BadRequest, missingHeader.StatusCode);

        var unknown = await _client.ConfirmHoldAsync(Guid.NewGuid(), TestData.StudentId());
        Assert.Equal(HttpStatusCode.NotFound, unknown.StatusCode);
        Assert.Equal("HOLD_NOT_FOUND", await unknown.ErrorCodeAsync());

        var malformed = await _client.CancelHoldAsync(Guid.Empty, TestData.StudentId());
        Assert.Equal(HttpStatusCode.NotFound, malformed.StatusCode);

        var badId = await _client.SendAsync(new HttpRequestMessage(HttpMethod.Delete, "/holds/abc")
            { Headers = { { ApiClientExtensions.StudentHeader, TestData.StudentId() } } });
        Assert.Equal(HttpStatusCode.BadRequest, badId.StatusCode);
        Assert.Equal("INVALID_ID", await badId.ErrorCodeAsync());
    }

    [Fact]
    public async Task Cancelling_releases_the_seat_and_is_idempotent()
    {
        var cls = await _client.CreateClassAsync(capacity: 1);
        var student = TestData.StudentId();
        var hold = await _client.PlaceHoldOkAsync(cls.Id, student);

        var first = await _client.CancelHoldAsync(hold.Id, student);
        var second = await _client.CancelHoldAsync(hold.Id, student);

        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Equal(HttpStatusCode.OK, second.StatusCode);
        Assert.Equal("CANCELLED", (await second.ReadAsync<HoldDto>()).Status);
        Assert.Equal(1, (await _client.GetClassAsync(cls.Id)).AvailableSeats);

        // Seat is free for someone else, and the same student may hold again.
        Assert.Equal(HttpStatusCode.Created, (await _client.PlaceHoldAsync(cls.Id, student)).StatusCode);
    }

    [Fact]
    public async Task Confirmed_admission_cannot_be_cancelled()
    {
        var cls = await _client.CreateClassAsync(capacity: 1);
        var student = TestData.StudentId();
        var hold = await _client.PlaceHoldOkAsync(cls.Id, student);
        await _client.ConfirmHoldAsync(hold.Id, student);

        var response = await _client.CancelHoldAsync(hold.Id, student);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal("HOLD_ALREADY_CONFIRMED", await response.ErrorCodeAsync());
    }

    [Fact]
    public async Task Expired_hold_cannot_be_cancelled()
    {
        var cls = await _client.CreateClassAsync(capacity: 1);
        var student = TestData.StudentId();
        var hold = await _client.PlaceHoldOkAsync(cls.Id, student);
        factory.Clock.Advance(TimeSpan.FromSeconds(ApiFactory.HoldDurationSeconds + 1));

        var response = await _client.CancelHoldAsync(hold.Id, student);

        Assert.Equal(HttpStatusCode.Gone, response.StatusCode);
        Assert.Equal("HOLD_EXPIRED", await response.ErrorCodeAsync());
    }

    [Fact]
    public async Task History_shows_every_state()
    {
        var classes = new[]
        {
            await _client.CreateClassAsync(capacity: 1),
            await _client.CreateClassAsync(capacity: 1),
            await _client.CreateClassAsync(capacity: 1),
        };
        var student = TestData.StudentId();

        var confirmed = await _client.PlaceHoldOkAsync(classes[0].Id, student);
        await _client.ConfirmHoldAsync(confirmed.Id, student);
        var cancelled = await _client.PlaceHoldOkAsync(classes[1].Id, student);
        await _client.CancelHoldAsync(cancelled.Id, student);
        var held = await _client.PlaceHoldOkAsync(classes[2].Id, student);

        var history = await (await _client.MyHoldsAsync(student)).ReadAsync<List<HoldDto>>();

        Assert.Equal(
            new Dictionary<Guid, string> { [confirmed.Id] = "CONFIRMED", [cancelled.Id] = "CANCELLED", [held.Id] = "HELD" },
            history.ToDictionary(h => h.Id, h => h.Status));
    }
}
