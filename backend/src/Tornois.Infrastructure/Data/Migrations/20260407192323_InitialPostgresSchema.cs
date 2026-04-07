using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Tornois.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialPostgresSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "admin");

            migrationBuilder.EnsureSchema(
                name: "broadcast");

            migrationBuilder.EnsureSchema(
                name: "sports");

            migrationBuilder.EnsureSchema(
                name: "rankings");

            migrationBuilder.EnsureSchema(
                name: "matches");

            migrationBuilder.EnsureSchema(
                name: "people");

            migrationBuilder.EnsureSchema(
                name: "stats");

            migrationBuilder.EnsureSchema(
                name: "teams");

            migrationBuilder.CreateTable(
                name: "admin_user",
                schema: "admin",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_name = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    display_name = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    role = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    password_hash = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    last_login_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_admin_user", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "broadcaster",
                schema: "broadcast",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    country = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    website_url = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_broadcaster", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "person",
                schema: "people",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    full_name = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    first_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    last_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    nationality = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    birth_date = table.Column<DateOnly>(type: "date", nullable: false),
                    photo_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    bio = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    primary_role = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_person", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "sport",
                schema: "sports",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    slug = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    is_olympic = table.Column<bool>(type: "boolean", nullable: false),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_sport", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "change_log",
                schema: "admin",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    admin_user_id = table.Column<int>(type: "integer", nullable: true),
                    action = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    entity_name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    summary = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    before_json = table.Column<string>(type: "jsonb", nullable: true),
                    after_json = table.Column<string>(type: "jsonb", nullable: true),
                    timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_change_log", x => x.id);
                    table.ForeignKey(
                        name: "f_k_change_log_admin_user_admin_user_id",
                        column: x => x.admin_user_id,
                        principalSchema: "admin",
                        principalTable: "admin_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "competition",
                schema: "sports",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    sport_id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    country = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    format = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    is_cup = table.Column<bool>(type: "boolean", nullable: false),
                    external_source = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    external_id = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_competition", x => x.id);
                    table.ForeignKey(
                        name: "f_k_competition_sport_sport_id",
                        column: x => x.sport_id,
                        principalSchema: "sports",
                        principalTable: "sport",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "team",
                schema: "teams",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    sport_id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(140)", maxLength: 140, nullable: false),
                    short_name = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    country = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    venue = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    founded = table.Column<int>(type: "integer", nullable: false),
                    badge_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_team", x => x.id);
                    table.ForeignKey(
                        name: "f_k_team_sport_sport_id",
                        column: x => x.sport_id,
                        principalSchema: "sports",
                        principalTable: "sport",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "season",
                schema: "sports",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    competition_id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    year_start = table.Column<int>(type: "integer", nullable: false),
                    year_end = table.Column<int>(type: "integer", nullable: false),
                    is_current = table.Column<bool>(type: "boolean", nullable: false),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_season", x => x.id);
                    table.ForeignKey(
                        name: "f_k_season_competition_competition_id",
                        column: x => x.competition_id,
                        principalSchema: "sports",
                        principalTable: "competition",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "individual_ranking",
                schema: "rankings",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    competition_id = table.Column<int>(type: "integer", nullable: false),
                    season_id = table.Column<int>(type: "integer", nullable: false),
                    category = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    label = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    updated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_individual_ranking", x => x.id);
                    table.ForeignKey(
                        name: "f_k_individual_ranking_competition_competition_id",
                        column: x => x.competition_id,
                        principalSchema: "sports",
                        principalTable: "competition",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "f_k_individual_ranking_season_season_id",
                        column: x => x.season_id,
                        principalSchema: "sports",
                        principalTable: "season",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "match",
                schema: "matches",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    sport_id = table.Column<int>(type: "integer", nullable: false),
                    competition_id = table.Column<int>(type: "integer", nullable: false),
                    season_id = table.Column<int>(type: "integer", nullable: false),
                    home_team_id = table.Column<int>(type: "integer", nullable: false),
                    away_team_id = table.Column<int>(type: "integer", nullable: false),
                    kickoff_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    home_score = table.Column<int>(type: "integer", nullable: false),
                    away_score = table.Column<int>(type: "integer", nullable: false),
                    venue = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    source = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_match", x => x.id);
                    table.ForeignKey(
                        name: "f_k_match_competition_competition_id",
                        column: x => x.competition_id,
                        principalSchema: "sports",
                        principalTable: "competition",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "f_k_match_season_season_id",
                        column: x => x.season_id,
                        principalSchema: "sports",
                        principalTable: "season",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "f_k_match_sport_sport_id",
                        column: x => x.sport_id,
                        principalSchema: "sports",
                        principalTable: "sport",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "f_k_match_team_away_team_id",
                        column: x => x.away_team_id,
                        principalSchema: "teams",
                        principalTable: "team",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "f_k_match_team_home_team_id",
                        column: x => x.home_team_id,
                        principalSchema: "teams",
                        principalTable: "team",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "standing",
                schema: "rankings",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    competition_id = table.Column<int>(type: "integer", nullable: false),
                    season_id = table.Column<int>(type: "integer", nullable: false),
                    team_id = table.Column<int>(type: "integer", nullable: false),
                    position = table.Column<int>(type: "integer", nullable: false),
                    played = table.Column<int>(type: "integer", nullable: false),
                    won = table.Column<int>(type: "integer", nullable: false),
                    drawn = table.Column<int>(type: "integer", nullable: false),
                    lost = table.Column<int>(type: "integer", nullable: false),
                    goals_for = table.Column<int>(type: "integer", nullable: false),
                    goals_against = table.Column<int>(type: "integer", nullable: false),
                    points = table.Column<int>(type: "integer", nullable: false),
                    updated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_standing", x => x.id);
                    table.ForeignKey(
                        name: "f_k_standing_competition_competition_id",
                        column: x => x.competition_id,
                        principalSchema: "sports",
                        principalTable: "competition",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "f_k_standing_season_season_id",
                        column: x => x.season_id,
                        principalSchema: "sports",
                        principalTable: "season",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "f_k_standing_team_team_id",
                        column: x => x.team_id,
                        principalSchema: "teams",
                        principalTable: "team",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "team_season",
                schema: "teams",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    team_id = table.Column<int>(type: "integer", nullable: false),
                    season_id = table.Column<int>(type: "integer", nullable: false),
                    coach_name = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_team_season", x => x.id);
                    table.ForeignKey(
                        name: "f_k_team_season_season_season_id",
                        column: x => x.season_id,
                        principalSchema: "sports",
                        principalTable: "season",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "f_k_team_season_team_team_id",
                        column: x => x.team_id,
                        principalSchema: "teams",
                        principalTable: "team",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "person_ranking",
                schema: "rankings",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ranking_id = table.Column<int>(type: "integer", nullable: false),
                    person_id = table.Column<int>(type: "integer", nullable: false),
                    team_id = table.Column<int>(type: "integer", nullable: false),
                    position = table.Column<int>(type: "integer", nullable: false),
                    value = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_person_ranking", x => x.id);
                    table.ForeignKey(
                        name: "f_k_person_ranking_individual_ranking_ranking_id",
                        column: x => x.ranking_id,
                        principalSchema: "rankings",
                        principalTable: "individual_ranking",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "f_k_person_ranking_person_person_id",
                        column: x => x.person_id,
                        principalSchema: "people",
                        principalTable: "person",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "f_k_person_ranking_team_team_id",
                        column: x => x.team_id,
                        principalSchema: "teams",
                        principalTable: "team",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "match_broadcast",
                schema: "broadcast",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    match_id = table.Column<int>(type: "integer", nullable: false),
                    broadcaster_id = table.Column<int>(type: "integer", nullable: false),
                    region = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    channel_name = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    stream_url = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_match_broadcast", x => x.id);
                    table.ForeignKey(
                        name: "f_k_match_broadcast_broadcaster_broadcaster_id",
                        column: x => x.broadcaster_id,
                        principalSchema: "broadcast",
                        principalTable: "broadcaster",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "f_k_match_broadcast_match_match_id",
                        column: x => x.match_id,
                        principalSchema: "matches",
                        principalTable: "match",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "match_event",
                schema: "matches",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    match_id = table.Column<int>(type: "integer", nullable: false),
                    minute = table.Column<int>(type: "integer", nullable: false),
                    type = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    subject = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    team_id = table.Column<int>(type: "integer", nullable: true),
                    person_id = table.Column<int>(type: "integer", nullable: true),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_match_event", x => x.id);
                    table.ForeignKey(
                        name: "f_k_match_event_match_match_id",
                        column: x => x.match_id,
                        principalSchema: "matches",
                        principalTable: "match",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "f_k_match_event_person_person_id",
                        column: x => x.person_id,
                        principalSchema: "people",
                        principalTable: "person",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "f_k_match_event_team_team_id",
                        column: x => x.team_id,
                        principalSchema: "teams",
                        principalTable: "team",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "player_stat",
                schema: "stats",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    match_id = table.Column<int>(type: "integer", nullable: false),
                    person_id = table.Column<int>(type: "integer", nullable: false),
                    team_id = table.Column<int>(type: "integer", nullable: false),
                    category = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    stat_payload_json = table.Column<string>(type: "jsonb", nullable: false),
                    metric_value = table.Column<decimal>(type: "numeric", nullable: false),
                    recorded_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_player_stat", x => x.id);
                    table.ForeignKey(
                        name: "f_k_player_stat_match_match_id",
                        column: x => x.match_id,
                        principalSchema: "matches",
                        principalTable: "match",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "f_k_player_stat_person_person_id",
                        column: x => x.person_id,
                        principalSchema: "people",
                        principalTable: "person",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "f_k_player_stat_team_team_id",
                        column: x => x.team_id,
                        principalSchema: "teams",
                        principalTable: "team",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "team_member",
                schema: "teams",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    team_season_id = table.Column<int>(type: "integer", nullable: false),
                    person_id = table.Column<int>(type: "integer", nullable: false),
                    squad_role = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    shirt_number = table.Column<int>(type: "integer", nullable: true),
                    joined_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_team_member", x => x.id);
                    table.ForeignKey(
                        name: "f_k_team_member_person_person_id",
                        column: x => x.person_id,
                        principalSchema: "people",
                        principalTable: "person",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "f_k_team_member_team_season_team_season_id",
                        column: x => x.team_season_id,
                        principalSchema: "teams",
                        principalTable: "team_season",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "i_x_admin_user_user_name",
                schema: "admin",
                table: "admin_user",
                column: "user_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_broadcaster_name",
                schema: "broadcast",
                table: "broadcaster",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_change_log_admin_user_id",
                schema: "admin",
                table: "change_log",
                column: "admin_user_id");

            migrationBuilder.CreateIndex(
                name: "i_x_competition_sport_id_name",
                schema: "sports",
                table: "competition",
                columns: new[] { "sport_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_individual_ranking_competition_id_season_id_category_label",
                schema: "rankings",
                table: "individual_ranking",
                columns: new[] { "competition_id", "season_id", "category", "label" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_individual_ranking_season_id",
                schema: "rankings",
                table: "individual_ranking",
                column: "season_id");

            migrationBuilder.CreateIndex(
                name: "i_x_match_away_team_id",
                schema: "matches",
                table: "match",
                column: "away_team_id");

            migrationBuilder.CreateIndex(
                name: "i_x_match_competition_id_season_id_kickoff_utc",
                schema: "matches",
                table: "match",
                columns: new[] { "competition_id", "season_id", "kickoff_utc" });

            migrationBuilder.CreateIndex(
                name: "i_x_match_home_team_id",
                schema: "matches",
                table: "match",
                column: "home_team_id");

            migrationBuilder.CreateIndex(
                name: "i_x_match_season_id",
                schema: "matches",
                table: "match",
                column: "season_id");

            migrationBuilder.CreateIndex(
                name: "i_x_match_sport_id",
                schema: "matches",
                table: "match",
                column: "sport_id");

            migrationBuilder.CreateIndex(
                name: "i_x_match_broadcast_broadcaster_id",
                schema: "broadcast",
                table: "match_broadcast",
                column: "broadcaster_id");

            migrationBuilder.CreateIndex(
                name: "i_x_match_broadcast_match_id",
                schema: "broadcast",
                table: "match_broadcast",
                column: "match_id");

            migrationBuilder.CreateIndex(
                name: "i_x_match_event_match_id",
                schema: "matches",
                table: "match_event",
                column: "match_id");

            migrationBuilder.CreateIndex(
                name: "i_x_match_event_person_id",
                schema: "matches",
                table: "match_event",
                column: "person_id");

            migrationBuilder.CreateIndex(
                name: "i_x_match_event_team_id",
                schema: "matches",
                table: "match_event",
                column: "team_id");

            migrationBuilder.CreateIndex(
                name: "i_x_person_full_name",
                schema: "people",
                table: "person",
                column: "full_name");

            migrationBuilder.CreateIndex(
                name: "i_x_person_ranking_person_id",
                schema: "rankings",
                table: "person_ranking",
                column: "person_id");

            migrationBuilder.CreateIndex(
                name: "i_x_person_ranking_ranking_id_person_id",
                schema: "rankings",
                table: "person_ranking",
                columns: new[] { "ranking_id", "person_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_person_ranking_team_id",
                schema: "rankings",
                table: "person_ranking",
                column: "team_id");

            migrationBuilder.CreateIndex(
                name: "i_x_player_stat_match_id_person_id_category",
                schema: "stats",
                table: "player_stat",
                columns: new[] { "match_id", "person_id", "category" });

            migrationBuilder.CreateIndex(
                name: "i_x_player_stat_person_id",
                schema: "stats",
                table: "player_stat",
                column: "person_id");

            migrationBuilder.CreateIndex(
                name: "i_x_player_stat_team_id",
                schema: "stats",
                table: "player_stat",
                column: "team_id");

            migrationBuilder.CreateIndex(
                name: "i_x_season_competition_id_name",
                schema: "sports",
                table: "season",
                columns: new[] { "competition_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_sport_slug",
                schema: "sports",
                table: "sport",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_standing_competition_id_season_id_team_id",
                schema: "rankings",
                table: "standing",
                columns: new[] { "competition_id", "season_id", "team_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_standing_season_id",
                schema: "rankings",
                table: "standing",
                column: "season_id");

            migrationBuilder.CreateIndex(
                name: "i_x_standing_team_id",
                schema: "rankings",
                table: "standing",
                column: "team_id");

            migrationBuilder.CreateIndex(
                name: "i_x_team_sport_id_name",
                schema: "teams",
                table: "team",
                columns: new[] { "sport_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_team_member_person_id",
                schema: "teams",
                table: "team_member",
                column: "person_id");

            migrationBuilder.CreateIndex(
                name: "i_x_team_member_team_season_id_person_id",
                schema: "teams",
                table: "team_member",
                columns: new[] { "team_season_id", "person_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_team_season_season_id",
                schema: "teams",
                table: "team_season",
                column: "season_id");

            migrationBuilder.CreateIndex(
                name: "i_x_team_season_team_id_season_id",
                schema: "teams",
                table: "team_season",
                columns: new[] { "team_id", "season_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "change_log",
                schema: "admin");

            migrationBuilder.DropTable(
                name: "match_broadcast",
                schema: "broadcast");

            migrationBuilder.DropTable(
                name: "match_event",
                schema: "matches");

            migrationBuilder.DropTable(
                name: "person_ranking",
                schema: "rankings");

            migrationBuilder.DropTable(
                name: "player_stat",
                schema: "stats");

            migrationBuilder.DropTable(
                name: "standing",
                schema: "rankings");

            migrationBuilder.DropTable(
                name: "team_member",
                schema: "teams");

            migrationBuilder.DropTable(
                name: "admin_user",
                schema: "admin");

            migrationBuilder.DropTable(
                name: "broadcaster",
                schema: "broadcast");

            migrationBuilder.DropTable(
                name: "individual_ranking",
                schema: "rankings");

            migrationBuilder.DropTable(
                name: "match",
                schema: "matches");

            migrationBuilder.DropTable(
                name: "person",
                schema: "people");

            migrationBuilder.DropTable(
                name: "team_season",
                schema: "teams");

            migrationBuilder.DropTable(
                name: "season",
                schema: "sports");

            migrationBuilder.DropTable(
                name: "team",
                schema: "teams");

            migrationBuilder.DropTable(
                name: "competition",
                schema: "sports");

            migrationBuilder.DropTable(
                name: "sport",
                schema: "sports");
        }
    }
}
