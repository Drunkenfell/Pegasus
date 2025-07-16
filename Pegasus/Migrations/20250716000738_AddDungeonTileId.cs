using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pegasus.Migrations
{
    /// <inheritdoc />
    public partial class AddDungeonTileId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "account",
                columns: table => new
                {
                    id = table.Column<uint>(type: "int unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    username = table.Column<string>(type: "varchar(50)", nullable: false, defaultValueSql: "''")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    password = table.Column<string>(type: "varchar(100)", nullable: false, defaultValueSql: "''")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    privileges = table.Column<short>(type: "smallint(6)", nullable: false, defaultValueSql: "'0'"),
                    createIp = table.Column<string>(type: "varchar(50)", nullable: false, defaultValueSql: "''")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    createTime = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "'CURRENT_TIMESTAMP'"),
                    lastIp = table.Column<string>(type: "varchar(50)", nullable: false, defaultValueSql: "''")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    lastTime = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "'CURRENT_TIMESTAMP'")
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_account", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "dungeon",
                columns: table => new
                {
                    landBlockId = table.Column<ushort>(type: "smallint unsigned", nullable: false, defaultValueSql: "'0'"),
                    name = table.Column<string>(type: "varchar(50)", nullable: false, defaultValueSql: "''")
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.landBlockId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "friend",
                columns: table => new
                {
                    id = table.Column<uint>(type: "int unsigned", nullable: false, defaultValueSql: "'0'"),
                    friend = table.Column<uint>(type: "int unsigned", nullable: false, defaultValueSql: "'0'"),
                    addTime = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "'CURRENT_TIMESTAMP'")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_friend", x => x.id);
                    table.ForeignKey(
                        name: "__FK_friend_friend__account_id",
                        column: x => x.friend,
                        principalTable: "account",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "__FK_friend_id__account_id",
                        column: x => x.id,
                        principalTable: "account",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "dungeon_tile",
                columns: table => new
                {
                    id = table.Column<uint>(type: "int unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    landBlockId = table.Column<ushort>(type: "smallint unsigned", nullable: false, defaultValueSql: "'0'"),
                    tileId = table.Column<ushort>(type: "smallint unsigned", nullable: false, defaultValueSql: "'0'"),
                    x = table.Column<float>(type: "float", nullable: false, defaultValueSql: "'0'"),
                    y = table.Column<float>(type: "float", nullable: false, defaultValueSql: "'0'"),
                    z = table.Column<float>(type: "float", nullable: false, defaultValueSql: "'0'")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dungeon_tile", x => x.id);
                    table.ForeignKey(
                        name: "__FK_dungeon_tile_landBlockId__dungeon_landBlockId",
                        column: x => x.landBlockId,
                        principalTable: "dungeon",
                        principalColumn: "landBlockId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "__FK_dungeon_tile_landBlockId__dungeon_landBlockId",
                table: "dungeon_tile",
                column: "landBlockId");

            migrationBuilder.CreateIndex(
                name: "__FK_friend_friend__account_id",
                table: "friend",
                column: "friend");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "dungeon_tile");

            migrationBuilder.DropTable(
                name: "friend");

            migrationBuilder.DropTable(
                name: "dungeon");

            migrationBuilder.DropTable(
                name: "account");
        }
    }
}
