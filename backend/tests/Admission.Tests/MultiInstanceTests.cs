using System.Net;
using Admission.Tests.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Admission.Tests;

[Collection(ApiCollection.Name)]
public sealed class MultiInstanceTests(ApiFactory factory)
{
    [Fact]
    public async Task Two_backend_instances_on_one_database_never_overbook()
    {
        await using var second = factory.WithWebHostBuilder(_ => { });
        var instanceA = factory.CreateClient();
        var instanceB = second.CreateClient();

        const int seats = 7, students = 60;
        var cls = await instanceA.CreateClassAsync(seats);

        // Half the students hit instance A, half hit instance B, all at the same moment.
        var responses = await ConcurrencyTests.RaceAsync(students, i =>
            (i % 2 == 0 ? instanceA : instanceB).PlaceHoldAsync(cls.Id, TestData.StudentId()));

        Assert.Equal(seats, responses.Count(r => r.StatusCode == HttpStatusCode.Created));
        Assert.Equal(students - seats, responses.Count(r => r.StatusCode == HttpStatusCode.Conflict));

        var viewA = await instanceA.GetClassAsync(cls.Id);
        var viewB = await instanceB.GetClassAsync(cls.Id);
        Assert.Equal((seats, 0), (viewA.HeldSeats, viewA.AvailableSeats));
        Assert.Equal(viewA, viewB);
    }

    [Fact]
    public async Task Same_hold_confirmed_through_both_instances_counts_once()
    {
        await using var second = factory.WithWebHostBuilder(_ => { });
        var instanceA = factory.CreateClient();
        var instanceB = second.CreateClient();

        var cls = await instanceA.CreateClassAsync(seats: 3);
        var student = TestData.StudentId();
        var hold = await instanceA.PlaceHoldOkAsync(cls.Id, student);

        var responses = await ConcurrencyTests.RaceAsync(20, i =>
            (i % 2 == 0 ? instanceA : instanceB).ConfirmHoldAsync(hold.HoldId, student));

        Assert.All(responses, r => Assert.Equal(HttpStatusCode.OK, r.StatusCode));
        Assert.Equal(1, (await instanceB.GetClassAsync(cls.Id)).ConfirmedSeats);
    }
}
