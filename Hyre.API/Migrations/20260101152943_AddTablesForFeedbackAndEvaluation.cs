using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hyre.API.Migrations
{
    /// <inheritdoc />
    public partial class AddTablesForFeedbackAndEvaluation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RecruiterDecision",
                table: "CandidateInterviewRounds",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RecruiterDecisionAt",
                table: "CandidateInterviewRounds",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RecruiterDecisionBy",
                table: "CandidateInterviewRounds",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CandidateInterviewFeedbacks",
                columns: table => new
                {
                    FeedbackID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CandidateRoundID = table.Column<int>(type: "int", nullable: false),
                    InterviewerID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    OverallComment = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CandidateInterviewFeedbacks", x => x.FeedbackID);
                    table.ForeignKey(
                        name: "FK_CandidateInterviewFeedbacks_AspNetUsers_InterviewerID",
                        column: x => x.InterviewerID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CandidateInterviewFeedbacks_CandidateInterviewRounds_CandidateRoundID",
                        column: x => x.CandidateRoundID,
                        principalTable: "CandidateInterviewRounds",
                        principalColumn: "CandidateRoundID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InterviewSkillRatings",
                columns: table => new
                {
                    RatingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FeedbackID = table.Column<int>(type: "int", nullable: false),
                    SkillID = table.Column<int>(type: "int", nullable: false),
                    Rating = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InterviewSkillRatings", x => x.RatingID);
                    table.ForeignKey(
                        name: "FK_InterviewSkillRatings_CandidateInterviewFeedbacks_FeedbackID",
                        column: x => x.FeedbackID,
                        principalTable: "CandidateInterviewFeedbacks",
                        principalColumn: "FeedbackID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InterviewSkillRatings_Skills_SkillID",
                        column: x => x.SkillID,
                        principalTable: "Skills",
                        principalColumn: "SkillID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CandidateInterviewFeedbacks_CandidateRoundID",
                table: "CandidateInterviewFeedbacks",
                column: "CandidateRoundID");

            migrationBuilder.CreateIndex(
                name: "IX_CandidateInterviewFeedbacks_InterviewerID",
                table: "CandidateInterviewFeedbacks",
                column: "InterviewerID");

            migrationBuilder.CreateIndex(
                name: "IX_InterviewSkillRatings_FeedbackID",
                table: "InterviewSkillRatings",
                column: "FeedbackID");

            migrationBuilder.CreateIndex(
                name: "IX_InterviewSkillRatings_SkillID",
                table: "InterviewSkillRatings",
                column: "SkillID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InterviewSkillRatings");

            migrationBuilder.DropTable(
                name: "CandidateInterviewFeedbacks");

            migrationBuilder.DropColumn(
                name: "RecruiterDecision",
                table: "CandidateInterviewRounds");

            migrationBuilder.DropColumn(
                name: "RecruiterDecisionAt",
                table: "CandidateInterviewRounds");

            migrationBuilder.DropColumn(
                name: "RecruiterDecisionBy",
                table: "CandidateInterviewRounds");
        }
    }
}
