using Admission.Api.Data;
using Admission.Api.Domain;
using Admission.Api.Http;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Admission.Api.Features.Classes;

public sealed class ClassService(AdmissionDbContext db, TimeProvider clock)
{
    public async Task<ClassResponse> CreateAsync(CreateClassRequest request, CancellationToken ct)
    {
        var (name, shift, capacity) = Validate(request);

        var exists = await db.Classes.AnyAsync(
            c => c.Shift == shift && c.Name.ToLower() == name.ToLower(), ct);
        if (exists) throw DuplicateClass(name, shift);

        var entity = new SchoolClass
        {
            Id = Guid.NewGuid(),
            Name = name,
            Shift = shift,
            Capacity = capacity,
            CreatedAt = clock.GetUtcNow(),
        };
        db.Classes.Add(entity);

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            throw DuplicateClass(name, shift);
        }

        return ClassResponse.From(entity);
    }

    public async Task<IReadOnlyList<ClassResponse>> ListAsync(CancellationToken ct)
    {
        var classes = await db.Classes.AsNoTracking()
            .OrderBy(c => c.Name).ThenBy(c => c.Shift)
            .ToListAsync(ct);
        return classes.Select(ClassResponse.From).ToList();
    }

    public async Task<ClassResponse> GetAsync(Guid id, CancellationToken ct)
    {
        var entity = await db.Classes.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, ct)
            ?? throw ApiException.NotFound("CLASS_NOT_FOUND", "Class not found.");
        return ClassResponse.From(entity);
    }

    private static (string Name, Shift Shift, int Capacity) Validate(CreateClassRequest request)
    {
        var name = request.Name?.Trim();
        if (string.IsNullOrEmpty(name))
            throw ApiException.BadRequest("VALIDATION_ERROR", "Name is required.");
        if (name.Length > SchoolClass.NameMaxLength)
            throw ApiException.BadRequest("VALIDATION_ERROR", $"Name must be at most {SchoolClass.NameMaxLength} characters.");

        if (string.IsNullOrWhiteSpace(request.Shift)
            || !Enum.TryParse<Shift>(request.Shift.Trim(), ignoreCase: true, out var shift)
            || !Enum.IsDefined(shift))
            throw ApiException.BadRequest("VALIDATION_ERROR",
                $"Shift must be one of: {string.Join(", ", Enum.GetNames<Shift>())}.");

        if (request.Capacity is not { } capacity
            || capacity < SchoolClass.MinCapacity || capacity > SchoolClass.MaxCapacity)
            throw ApiException.BadRequest("VALIDATION_ERROR",
                $"Capacity must be between {SchoolClass.MinCapacity} and {SchoolClass.MaxCapacity}.");

        return (name, shift, capacity);
    }

    private static ApiException DuplicateClass(string name, Shift shift) =>
        ApiException.Conflict("CLASS_ALREADY_EXISTS", $"Class '{name}' already exists in the {shift} shift.");
}
