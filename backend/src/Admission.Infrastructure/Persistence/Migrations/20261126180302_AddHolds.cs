using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Admission.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddHolds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "holds",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    class_id = table.Column<Guid>(type: "uuid", nullable: false),
                    student_id = table.Column<string>(type: "character varying(17)", maxLength: 17, nullable: false),
                    status = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    confirmed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    cancelled_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_holds", x => x.id);
                    table.CheckConstraint("ck_holds_status", "status IN ('HELD','CONFIRMED','EXPIRED','CANCELLED')");
                    table.ForeignKey(
                        name: "FK_holds_classes_class_id",
                        column: x => x.class_id,
                        principalTable: "classes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_holds_class_status_expires",
                table: "holds",
                columns: new[] { "class_id", "status", "expires_at" });

            migrationBuilder.CreateIndex(
                name: "ix_holds_status_expires",
                table: "holds",
                columns: new[] { "status", "expires_at" });

            migrationBuilder.CreateIndex(
                name: "ix_holds_student_created",
                table: "holds",
                columns: new[] { "student_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ux_holds_one_active_per_student_class",
                table: "holds",
                columns: new[] { "class_id", "student_id" },
                unique: true,
                filter: "status IN ('HELD','CONFIRMED')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "holds");
        }
    }
}
