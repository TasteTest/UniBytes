using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend_loyalty.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "loyalty_accounts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    points_balance = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L),
                    tier = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_loyalty_accounts", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "loyalty_redemptions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    loyalty_account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    points_used = table.Column<long>(type: "bigint", nullable: false),
                    reward_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    reward_metadata = table.Column<string>(type: "jsonb", nullable: false, defaultValue: "{}"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_loyalty_redemptions", x => x.id);
                    table.ForeignKey(
                        name: "FK_loyalty_redemptions_loyalty_accounts_loyalty_account_id",
                        column: x => x.loyalty_account_id,
                        principalTable: "loyalty_accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "loyalty_transactions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    loyalty_account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    change_amount = table.Column<long>(type: "bigint", nullable: false),
                    reason = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    reference_id = table.Column<Guid>(type: "uuid", nullable: true),
                    metadata = table.Column<string>(type: "jsonb", nullable: false, defaultValue: "{}"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_loyalty_transactions", x => x.id);
                    table.ForeignKey(
                        name: "FK_loyalty_transactions_loyalty_accounts_loyalty_account_id",
                        column: x => x.loyalty_account_id,
                        principalTable: "loyalty_accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "idx_loyalty_accounts_created_at",
                table: "loyalty_accounts",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "idx_loyalty_accounts_is_active",
                table: "loyalty_accounts",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "idx_loyalty_accounts_tier",
                table: "loyalty_accounts",
                column: "tier");

            migrationBuilder.CreateIndex(
                name: "idx_loyalty_accounts_user_id",
                table: "loyalty_accounts",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_loyalty_redemptions_account_id",
                table: "loyalty_redemptions",
                column: "loyalty_account_id");

            migrationBuilder.CreateIndex(
                name: "idx_loyalty_redemptions_created_at",
                table: "loyalty_redemptions",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "idx_loyalty_redemptions_reward_type",
                table: "loyalty_redemptions",
                column: "reward_type");

            migrationBuilder.CreateIndex(
                name: "idx_loyalty_transactions_account_id",
                table: "loyalty_transactions",
                column: "loyalty_account_id");

            migrationBuilder.CreateIndex(
                name: "idx_loyalty_transactions_created_at",
                table: "loyalty_transactions",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "idx_loyalty_transactions_reference_id",
                table: "loyalty_transactions",
                column: "reference_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "loyalty_redemptions");

            migrationBuilder.DropTable(
                name: "loyalty_transactions");

            migrationBuilder.DropTable(
                name: "loyalty_accounts");
        }
    }
}
