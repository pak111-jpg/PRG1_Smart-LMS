using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchulApp.Migrations
{
    /// <inheritdoc />
    public partial class InitialDbCreation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Fach",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false, collation: "NOCASE"),
                    Lehrperson = table.Column<string>(type: "TEXT", nullable: false),
                    Erstellt = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fach", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Dokument",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Titel = table.Column<string>(type: "TEXT", nullable: false, collation: "NOCASE"),
                    Typ = table.Column<string>(type: "TEXT", nullable: false),
                    Dateipfad = table.Column<string>(type: "TEXT", nullable: true),
                    Frist = table.Column<string>(type: "TEXT", nullable: true),
                    Abgegeben = table.Column<bool>(type: "INTEGER", nullable: false),
                    FachId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dokument", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Dokument_Fach_FachId",
                        column: x => x.FachId,
                        principalTable: "Fach",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Dokument_FachId",
                table: "Dokument",
                column: "FachId");

            migrationBuilder.CreateIndex(
                name: "IX_Dokument_Frist",
                table: "Dokument",
                column: "Frist");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Dokument");

            migrationBuilder.DropTable(
                name: "Fach");
        }
    }
}
