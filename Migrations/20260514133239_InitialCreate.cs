using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Notify.Migrations
{
    // Migração gerada automaticamente pelo EF Core com: dotnet ef migrations add InitialCreate
    // Aplicada à base de dados com: dotnet ef database update
    public partial class InitialCreate : Migration
    {
        // Up: o que fazer ao aplicar a migração (criar a tabela Notes)
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Notes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    Content = table.Column<string>(type: "TEXT", nullable: false),
                    Summary = table.Column<string>(type: "TEXT", nullable: true),
                    Tags = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notes", x => x.Id);
                });
        }

        // Down: como reverter — usado em dotnet ef database update <migração-anterior>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Notes");
        }
    }
}
