using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Admission.Infrastructure.Persistence.Migrations
{
    public partial class EachShiftIsAClass : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ux_classes_normalized_name_shift",
                table: "classes");

            migrationBuilder.DropCheckConstraint(
                name: "ck_classes_capacity_positive",
                table: "classes");

            // Each shift becomes its own class: keep existing rows unique and meaningful by folding
            // the shift into the name, e.g. "Class 6" + MORNING -> "Class 6 - Morning Shift".
            migrationBuilder.Sql("""
                UPDATE classes
                SET name = left(name || ' - ' || initcap(shift) || ' Shift', 100),
                    normalized_name = upper(left(name || ' - ' || initcap(shift) || ' Shift', 100));
                """);

            migrationBuilder.DropColumn(
                name: "shift",
                table: "classes");

            migrationBuilder.RenameColumn(
                name: "capacity",
                table: "classes",
                newName: "total_seats");

            migrationBuilder.CreateIndex(
                name: "ux_classes_normalized_name",
                table: "classes",
                column: "normalized_name",
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "ck_classes_total_seats_positive",
                table: "classes",
                sql: "total_seats > 0");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ux_classes_normalized_name",
                table: "classes");

            migrationBuilder.DropCheckConstraint(
                name: "ck_classes_total_seats_positive",
                table: "classes");

            migrationBuilder.RenameColumn(
                name: "total_seats",
                table: "classes",
                newName: "capacity");

            migrationBuilder.AddColumn<string>(
                name: "shift",
                table: "classes",
                type: "character varying(16)",
                maxLength: 16,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "ux_classes_normalized_name_shift",
                table: "classes",
                columns: new[] { "normalized_name", "shift" },
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "ck_classes_capacity_positive",
                table: "classes",
                sql: "capacity > 0");
        }
    }
}
