using System.Net;
using Admission.Tests.Infrastructure;

namespace Admission.Tests;

[Collection(ApiCollection.Name)]
public sealed class AdmissionConcurrencyTests(ApiFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Parallel_confirmations_of_one_hold_produce_one_admission()
    {
        var cls = await _client.CreateClassAsync(seats: 1);
        var student = TestData.StudentId();
        var hold = await _client.PlaceHoldOkAsync(cls.Id, student);

        var responses = await ConcurrencyTests.RaceAsync(15, _ => _client.ConfirmHoldAsync(hold.Id, student));

        Assert.All(responses, r => Assert.Equal(HttpStatusCode.OK, r.StatusCode));
        var confirmedAt = await Task.WhenAll(responses.Select(async r => (await r.ReadAsync<HoldDto>()).ConfirmedAt));
        Assert.Single(confirmedAt.Distinct());
        var after = await _client.GetClassAsync(cls.Id);
        Assert.Equal((1, 0), (after.ConfirmedSeats, after.HeldSeats));
    }

    [Fact]
    public async Task Parallel_cancellations_are_all_safe()
    {
        var cls = await _client.CreateClassAsync(seats: 1);
        var student = TestData.StudentId();
        var hold = await _client.PlaceHoldOkAsync(cls.Id, student);

        var responses = await ConcurrencyTests.RaceAsync(15, _ => _client.CancelHoldAsync(hold.Id, student));

        Assert.All(responses, r => Assert.Equal(HttpStatusCode.NoContent, r.StatusCode));
        Assert.Equal(1, (await _client.GetClassAsync(cls.Id)).AvailableSeats);
    }

    [Fact]
    public async Task Racing_confirm_and_cancel_end_in_exactly_one_terminal_state()
    {
        for (var round = 0; round < 10; round++)
        {
            var cls = await _client.CreateClassAsync(seats: 1);
            var student = TestData.StudentId();
            var hold = await _client.PlaceHoldOkAsync(cls.Id, student);

            var responses = await ConcurrencyTests.RaceAsync(2, i => i == 0
                ? _client.ConfirmHoldAsync(hold.Id, student)
                : _client.CancelHoldAsync(hold.Id, student));

            // Confirm succeeds with 200, cancel with 204; whichever loses gets 409.
            Assert.Single(responses, r => r.IsSuccessStatusCode);
            Assert.Single(responses, r => r.StatusCode == HttpStatusCode.Conflict);

            var final = await (await _client.GetHoldAsync(hold.Id, student)).ReadAsync<HoldDto>();
            var after = await _client.GetClassAsync(cls.Id);
            if (final.Status == "CONFIRMED") Assert.Equal(1, after.ConfirmedSeats);
            else Assert.Equal(("CANCELLED", 1), (final.Status, after.AvailableSeats));
        }
    }

    [Fact]
    public async Task Released_seats_are_never_handed_out_twice()
    {
        // Capacity 3: three students hold, two cancel, then 20 newcomers race for the two freed seats.
        var cls = await _client.CreateClassAsync(seats: 3);
        var holders = Enumerable.Range(0, 3).Select(_ => TestData.StudentId()).ToArray();
        var holds = new List<HoldDto>();
        foreach (var s in holders) holds.Add(await _client.PlaceHoldOkAsync(cls.Id, s));

        await _client.CancelHoldAsync(holds[0].Id, holders[0]);
        await _client.CancelHoldAsync(holds[1].Id, holders[1]);

        var responses = await ConcurrencyTests.RaceAsync(20, _ => _client.PlaceHoldAsync(cls.Id, TestData.StudentId()));

        Assert.Equal(2, responses.Count(r => r.StatusCode == HttpStatusCode.Created));
        var after = await _client.GetClassAsync(cls.Id);
        Assert.Equal((3, 0), (after.HeldSeats, after.AvailableSeats));
    }
}
