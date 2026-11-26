using System.Net;
using Admission.Tests.Infrastructure;

namespace Admission.Tests;

/// <summary>
/// Fires many requests at the same moment against a real PostgreSQL database
/// to prove that capacity and uniqueness hold under contention.
/// </summary>
[Collection(ApiCollection.Name)]
public sealed class ConcurrencyTests(ApiFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Fifty_students_racing_for_five_seats_never_overbook()
    {
        const int capacity = 5, students = 50;
        var cls = await _client.CreateClassAsync(capacity);

        var responses = await RaceAsync(students, _ => _client.PlaceHoldAsync(cls.Id, TestData.StudentId()));

        Assert.Equal(capacity, responses.Count(r => r.StatusCode == HttpStatusCode.Created));
        Assert.Equal(students - capacity, responses.Count(r => r.StatusCode == HttpStatusCode.Conflict));
        var after = await _client.GetClassAsync(cls.Id);
        Assert.Equal((capacity, 0), (after.HeldSeats, after.AvailableSeats));
    }

    [Fact]
    public async Task Two_students_racing_for_the_last_seat_only_one_wins()
    {
        var cls = await _client.CreateClassAsync(seats: 2);
        await _client.PlaceHoldOkAsync(cls.Id, TestData.StudentId());

        for (var round = 0; round < 5; round++)
        {
            // Fresh class each round with exactly one seat left.
            var c = await _client.CreateClassAsync(seats: 2);
            await _client.PlaceHoldOkAsync(c.Id, TestData.StudentId());

            var responses = await RaceAsync(2, _ => _client.PlaceHoldAsync(c.Id, TestData.StudentId()));

            Assert.Single(responses, r => r.StatusCode == HttpStatusCode.Created);
            var loser = Assert.Single(responses, r => r.StatusCode == HttpStatusCode.Conflict);
            Assert.Equal("NO_SEATS_AVAILABLE", await loser.ErrorCodeAsync());
        }
    }

    [Fact]
    public async Task Same_student_sending_parallel_holds_gets_exactly_one()
    {
        var cls = await _client.CreateClassAsync(seats: 10);
        var student = TestData.StudentId();

        var responses = await RaceAsync(20, _ => _client.PlaceHoldAsync(cls.Id, student));

        Assert.Single(responses, r => r.StatusCode == HttpStatusCode.Created);
        Assert.All(responses.Where(r => r.StatusCode != HttpStatusCode.Created),
            r => Assert.Equal(HttpStatusCode.Conflict, r.StatusCode));
        Assert.Equal(1, (await _client.GetClassAsync(cls.Id)).HeldSeats);
    }

    [Fact]
    public async Task Parallel_load_across_several_classes_respects_each_capacity()
    {
        var classes = new[]
        {
            await _client.CreateClassAsync(seats: 1),
            await _client.CreateClassAsync(seats: 3),
            await _client.CreateClassAsync(seats: 7),
        };

        await RaceAsync(90, i => _client.PlaceHoldAsync(classes[i % classes.Length].Id, TestData.StudentId()));

        foreach (var c in classes)
        {
            var after = await _client.GetClassAsync(c.Id);
            Assert.Equal(c.TotalSeats, after.HeldSeats);
            Assert.Equal(0, after.AvailableSeats);
        }
    }

    /// <summary>Starts all requests together behind a barrier so they truly overlap.</summary>
    internal static async Task<HttpResponseMessage[]> RaceAsync(int count, Func<int, Task<HttpResponseMessage>> send)
    {
        using var gate = new SemaphoreSlim(0);
        var tasks = Enumerable.Range(0, count).Select(async i =>
        {
            await gate.WaitAsync();
            return await send(i);
        }).ToArray();
        gate.Release(count);
        return await Task.WhenAll(tasks);
    }
}
