using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CampusLearn.Migrations
{
    /// <inheritdoc />
    public partial class UpdateLessonProgressDeleteBehavior : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LessonProgresses_Enrollments_EnrollmentId",
                table: "LessonProgresses");

            migrationBuilder.AddForeignKey(
                name: "FK_LessonProgresses_Enrollments_EnrollmentId",
                table: "LessonProgresses",
                column: "EnrollmentId",
                principalTable: "Enrollments",
                principalColumn: "EnrollmentId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LessonProgresses_Enrollments_EnrollmentId",
                table: "LessonProgresses");

            migrationBuilder.AddForeignKey(
                name: "FK_LessonProgresses_Enrollments_EnrollmentId",
                table: "LessonProgresses",
                column: "EnrollmentId",
                principalTable: "Enrollments",
                principalColumn: "EnrollmentId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
