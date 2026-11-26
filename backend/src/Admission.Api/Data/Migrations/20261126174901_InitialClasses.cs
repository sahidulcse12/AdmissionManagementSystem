using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Admission.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialClasses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "classes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    shift = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    capacity = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_classes", x => x.id);
                    table.CheckConstraint("ck_classes_capacity_positive", "capacity > 0");
                });

            migrationBuilder.CreateIndex(
                name: "ux_classes_name_shift",
                table: "classes",
                columns: new[] { "name", "shift" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "classes");
        }
    }
}
