using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Configuration.Migrations
{
    /// <inheritdoc />
    public partial class initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "watchers",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    search_pattern = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    executed_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    modified_files = table.Column<int>(type: "INTEGER", nullable: false),
                    input_path = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    output_path = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    is_enabled = table.Column<bool>(type: "INTEGER", nullable: true),
                    action = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_watchers", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "watchers");
        }
    }
}
