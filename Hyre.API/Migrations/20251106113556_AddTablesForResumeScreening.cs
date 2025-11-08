using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hyre.API.Migrations
{
    /// <inheritdoc />
    public partial class AddTablesForResumeScreening : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CandidateReviews",
                columns: table => new
                {
                    ReviewID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CandidateJobID = table.Column<int>(type: "int", nullable: false),
                    ReviewerId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Decision = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RecruiterId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    RecruiterDecision = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RecruiterActionAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CandidateReviews", x => x.ReviewID);
                    table.ForeignKey(
                        name: "FK_CandidateReviews_AspNetUsers_RecruiterId",
                        column: x => x.RecruiterId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CandidateReviews_AspNetUsers_ReviewerId",
                        column: x => x.ReviewerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CandidateReviews_CandidateJobs_CandidateJobID",
                        column: x => x.CandidateJobID,
                        principalTable: "CandidateJobs",
                        principalColumn: "CandidateJobID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "JobReviewers",
                columns: table => new
                {
                    JobReviewerId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JobId = table.Column<int>(type: "int", nullable: false),
                    ReviewerId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AssignedBy = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobReviewers", x => x.JobReviewerId);
                    table.ForeignKey(
                        name: "FK_JobReviewers_AspNetUsers_AssignedBy",
                        column: x => x.AssignedBy,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_JobReviewers_AspNetUsers_ReviewerId",
                        column: x => x.ReviewerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_JobReviewers_Jobs_JobId",
                        column: x => x.JobId,
                        principalTable: "Jobs",
                        principalColumn: "JobID",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "CandidateReviewComments",
                columns: table => new
                {
                    CommentID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CandidateReviewID = table.Column<int>(type: "int", nullable: false),
                    CommenterId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CommentText = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    CommentedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CandidateReviewComments", x => x.CommentID);
                    table.ForeignKey(
                        name: "FK_CandidateReviewComments_AspNetUsers_CommenterId",
                        column: x => x.CommenterId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_CandidateReviewComments_CandidateReviews_CandidateReviewID",
                        column: x => x.CandidateReviewID,
                        principalTable: "CandidateReviews",
                        principalColumn: "ReviewID",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "CandidateSkillReviews",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CandidateReviewId = table.Column<int>(type: "int", nullable: false),
                    SkillId = table.Column<int>(type: "int", nullable: false),
                    IsVerified = table.Column<bool>(type: "bit", nullable: false),
                    VerifiedYearsOfExperience = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CandidateSkillReviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CandidateSkillReviews_CandidateReviews_CandidateReviewId",
                        column: x => x.CandidateReviewId,
                        principalTable: "CandidateReviews",
                        principalColumn: "ReviewID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CandidateSkillReviews_Skills_SkillId",
                        column: x => x.SkillId,
                        principalTable: "Skills",
                        principalColumn: "SkillID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CandidateReviewComments_CandidateReviewID",
                table: "CandidateReviewComments",
                column: "CandidateReviewID");

            migrationBuilder.CreateIndex(
                name: "IX_CandidateReviewComments_CommenterId",
                table: "CandidateReviewComments",
                column: "CommenterId");

            migrationBuilder.CreateIndex(
                name: "IX_CandidateReviews_CandidateJobID",
                table: "CandidateReviews",
                column: "CandidateJobID");

            migrationBuilder.CreateIndex(
                name: "IX_CandidateReviews_RecruiterId",
                table: "CandidateReviews",
                column: "RecruiterId");

            migrationBuilder.CreateIndex(
                name: "IX_CandidateReviews_ReviewerId",
                table: "CandidateReviews",
                column: "ReviewerId");

            migrationBuilder.CreateIndex(
                name: "IX_CandidateSkillReviews_CandidateReviewId",
                table: "CandidateSkillReviews",
                column: "CandidateReviewId");

            migrationBuilder.CreateIndex(
                name: "IX_CandidateSkillReviews_SkillId",
                table: "CandidateSkillReviews",
                column: "SkillId");

            migrationBuilder.CreateIndex(
                name: "IX_JobReviewers_AssignedBy",
                table: "JobReviewers",
                column: "AssignedBy");

            migrationBuilder.CreateIndex(
                name: "IX_JobReviewers_JobId",
                table: "JobReviewers",
                column: "JobId");

            migrationBuilder.CreateIndex(
                name: "IX_JobReviewers_ReviewerId",
                table: "JobReviewers",
                column: "ReviewerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CandidateReviewComments");

            migrationBuilder.DropTable(
                name: "CandidateSkillReviews");

            migrationBuilder.DropTable(
                name: "JobReviewers");

            migrationBuilder.DropTable(
                name: "CandidateReviews");
        }
    }
}
