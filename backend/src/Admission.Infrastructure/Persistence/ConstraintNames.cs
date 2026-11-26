namespace Admission.Infrastructure.Persistence;

/// <summary>Names of database constraints and indexes referenced from code.</summary>
public static class ConstraintNames
{
    public const string ClassNameShift = "ux_classes_normalized_name_shift";
    public const string OneActiveHoldPerStudentPerClass = "ux_holds_one_active_per_student_class";
}
