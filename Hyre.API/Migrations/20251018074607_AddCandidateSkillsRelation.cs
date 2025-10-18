using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hyre.API.Migrations
{
    /// <inheritdoc />
    public partial class AddCandidateSkillsRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CandidateSkills",
                columns: table => new
                {
                    CandidateSkillID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CandidateID = table.Column<int>(type: "int", nullable: false),
                    SkillID = table.Column<int>(type: "int", nullable: false),
                    YearsOfExperience = table.Column<decimal>(type: "decimal(4,1)", precision: 4, scale: 1, nullable: true),
                    AddedBy = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    AddedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CandidateSkills", x => x.CandidateSkillID);
                    table.ForeignKey(
                        name: "FK_CandidateSkills_AspNetUsers_AddedBy",
                        column: x => x.AddedBy,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CandidateSkills_Candidates_CandidateID",
                        column: x => x.CandidateID,
                        principalTable: "Candidates",
                        principalColumn: "CandidateID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CandidateSkills_Skills_SkillID",
                        column: x => x.SkillID,
                        principalTable: "Skills",
                        principalColumn: "SkillID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CandidateSkills_AddedBy",
                table: "CandidateSkills",
                column: "AddedBy");

            migrationBuilder.CreateIndex(
                name: "IX_CandidateSkills_CandidateID",
                table: "CandidateSkills",
                column: "CandidateID");

            migrationBuilder.CreateIndex(
                name: "IX_CandidateSkills_SkillID",
                table: "CandidateSkills",
                column: "SkillID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CandidateSkills");
        }
    }
}
