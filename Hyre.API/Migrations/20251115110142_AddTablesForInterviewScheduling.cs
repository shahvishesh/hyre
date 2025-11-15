using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hyre.API.Migrations
{
    /// <inheritdoc />
    public partial class AddTablesForInterviewScheduling : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CandidateInterviewRounds",
                columns: table => new
                {
                    CandidateRoundID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CandidateID = table.Column<int>(type: "int", nullable: false),
                    JobID = table.Column<int>(type: "int", nullable: false),
                    SequenceNo = table.Column<int>(type: "int", nullable: false),
                    RoundName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RoundType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RecruiterID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    InterviewerID = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ScheduledDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StartTime = table.Column<TimeSpan>(type: "time", nullable: true),
                    DurationMinutes = table.Column<int>(type: "int", nullable: true),
                    InterviewMode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    MeetingLink = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    IsPanelRound = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CandidateInterviewRounds", x => x.CandidateRoundID);
                    table.ForeignKey(
                        name: "FK_CandidateInterviewRounds_AspNetUsers_InterviewerID",
                        column: x => x.InterviewerID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CandidateInterviewRounds_AspNetUsers_RecruiterID",
                        column: x => x.RecruiterID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_CandidateInterviewRounds_Candidates_CandidateID",
                        column: x => x.CandidateID,
                        principalTable: "Candidates",
                        principalColumn: "CandidateID",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_CandidateInterviewRounds_Jobs_JobID",
                        column: x => x.JobID,
                        principalTable: "Jobs",
                        principalColumn: "JobID",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "JobInterviewers",
                columns: table => new
                {
                    JobInterviewerID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JobID = table.Column<int>(type: "int", nullable: false),
                    InterviewerID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SkillArea = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    AssignedBy = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobInterviewers", x => x.JobInterviewerID);
                    table.ForeignKey(
                        name: "FK_JobInterviewers_AspNetUsers_AssignedBy",
                        column: x => x.AssignedBy,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_JobInterviewers_AspNetUsers_InterviewerID",
                        column: x => x.InterviewerID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_JobInterviewers_Jobs_JobID",
                        column: x => x.JobID,
                        principalTable: "Jobs",
                        principalColumn: "JobID",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "JobInterviewRoundTemplates",
                columns: table => new
                {
                    RoundTemplateID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JobID = table.Column<int>(type: "int", nullable: false),
                    SequenceNo = table.Column<int>(type: "int", nullable: false),
                    RoundName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RoundType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DurationMinutes = table.Column<int>(type: "int", nullable: false),
                    InterviewMode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsPanelRound = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobInterviewRoundTemplates", x => x.RoundTemplateID);
                    table.ForeignKey(
                        name: "FK_JobInterviewRoundTemplates_Jobs_JobID",
                        column: x => x.JobID,
                        principalTable: "Jobs",
                        principalColumn: "JobID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CandidatePanelMembers",
                columns: table => new
                {
                    PanelMemberID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CandidateRoundID = table.Column<int>(type: "int", nullable: false),
                    InterviewerID = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CandidatePanelMembers", x => x.PanelMemberID);
                    table.ForeignKey(
                        name: "FK_CandidatePanelMembers_AspNetUsers_InterviewerID",
                        column: x => x.InterviewerID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CandidatePanelMembers_CandidateInterviewRounds_CandidateRoundID",
                        column: x => x.CandidateRoundID,
                        principalTable: "CandidateInterviewRounds",
                        principalColumn: "CandidateRoundID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CandidateInterviewRounds_CandidateID",
                table: "CandidateInterviewRounds",
                column: "CandidateID");

            migrationBuilder.CreateIndex(
                name: "IX_CandidateInterviewRounds_InterviewerID",
                table: "CandidateInterviewRounds",
                column: "InterviewerID");

            migrationBuilder.CreateIndex(
                name: "IX_CandidateInterviewRounds_JobID",
                table: "CandidateInterviewRounds",
                column: "JobID");

            migrationBuilder.CreateIndex(
                name: "IX_CandidateInterviewRounds_RecruiterID",
                table: "CandidateInterviewRounds",
                column: "RecruiterID");

            migrationBuilder.CreateIndex(
                name: "IX_CandidatePanelMembers_CandidateRoundID",
                table: "CandidatePanelMembers",
                column: "CandidateRoundID");

            migrationBuilder.CreateIndex(
                name: "IX_CandidatePanelMembers_InterviewerID",
                table: "CandidatePanelMembers",
                column: "InterviewerID");

            migrationBuilder.CreateIndex(
                name: "IX_JobInterviewers_AssignedBy",
                table: "JobInterviewers",
                column: "AssignedBy");

            migrationBuilder.CreateIndex(
                name: "IX_JobInterviewers_InterviewerID",
                table: "JobInterviewers",
                column: "InterviewerID");

            migrationBuilder.CreateIndex(
                name: "IX_JobInterviewers_JobID",
                table: "JobInterviewers",
                column: "JobID");

            migrationBuilder.CreateIndex(
                name: "IX_JobInterviewRoundTemplates_JobID",
                table: "JobInterviewRoundTemplates",
                column: "JobID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CandidatePanelMembers");

            migrationBuilder.DropTable(
                name: "JobInterviewers");

            migrationBuilder.DropTable(
                name: "JobInterviewRoundTemplates");

            migrationBuilder.DropTable(
                name: "CandidateInterviewRounds");
        }
    }
}
