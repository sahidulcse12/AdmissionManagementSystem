using Admission.Application.Abstractions;
using Admission.Application.Common;
using Admission.Domain.Classes;

namespace Admission.Application.Classes;

/// <summary>Use cases for creating and reading classes.</summary>
public sealed class ClassService(IClassRepository classes, IUnitOfWork unitOfWork, TimeProvider clock)
{
    public async Task<ClassResponse> CreateAsync(CreateClassRequest request, CancellationToken ct)
    {
        var entity = SchoolClass.Create(request.Name, request.Shift, request.Capacity, clock.GetUtcNow());

        if (await classes.ExistsAsync(entity.NormalizedName, entity.Shift, ct))
            throw Duplicate(entity);

        classes.Add(entity);
        try
        {
            await unitOfWork.SaveChangesAsync(ct);
        }
        catch (UniqueConstraintException)
        {
            // Lost a race with a concurrent create of the same class.
            throw Duplicate(entity);
        }

        return ClassResponse.From(new ClassWithUsage(entity, 0, 0));
    }

    public async Task<IReadOnlyList<ClassResponse>> ListAsync(CancellationToken ct) =>
        (await classes.ListWithUsageAsync(clock.GetUtcNow(), ct)).Select(ClassResponse.From).ToList();

    public async Task<ClassResponse> GetAsync(Guid id, CancellationToken ct) =>
        ClassResponse.From(await classes.GetWithUsageAsync(id, clock.GetUtcNow(), ct) ?? throw NotFound());

    internal static NotFoundException NotFound() => new("CLASS_NOT_FOUND", "Class not found.");

    private static ConflictException Duplicate(SchoolClass c) =>
        new("CLASS_ALREADY_EXISTS", $"Class '{c.Name}' already exists in the {c.Shift} shift.");
}
