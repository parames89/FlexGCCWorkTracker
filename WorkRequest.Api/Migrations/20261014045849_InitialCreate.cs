using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkRequest.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WorkRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ClientName = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkRequests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorkRequestNotes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkRequestId = table.Column<int>(type: "int", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkRequestNotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkRequestNotes_WorkRequests_WorkRequestId",
                        column: x => x.WorkRequestId,
                        principalTable: "WorkRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorkRequestNotes_WorkRequestId",
                table: "WorkRequestNotes",
                column: "WorkRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkRequests_ClientName",
                table: "WorkRequests",
                column: "ClientName");

            migrationBuilder.CreateIndex(
                name: "IX_WorkRequests_Status_CreatedDate",
                table: "WorkRequests",
                columns: new[] { "Status", "CreatedDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WorkRequestNotes");

            migrationBuilder.DropTable(
                name: "WorkRequests");
        }
    }
}
