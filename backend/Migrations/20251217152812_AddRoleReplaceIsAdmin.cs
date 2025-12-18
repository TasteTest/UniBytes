using System;
using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddRoleReplaceIsAdmin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "idempotency_keys",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    key = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_idempotency_keys", x => x.id);
                });

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
                name: "menu_categories",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    display_order = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_menu_categories", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "orders",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    external_user_ref = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    total_amount = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    payment_status = table.Column<int>(type: "integer", nullable: false),
                    order_status = table.Column<int>(type: "integer", nullable: false),
                    placed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    cancel_requested_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    canceled_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    metadata = table.Column<JsonElement>(type: "jsonb", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_orders", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "payments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    order_id = table.Column<Guid>(type: "uuid", nullable: true),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    amount = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "USD"),
                    provider = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    provider_payment_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    provider_charge_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    raw_provider_response = table.Column<string>(type: "jsonb", nullable: true),
                    failure_message = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payments", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    first_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    last_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    bio = table.Column<string>(type: "text", nullable: true),
                    location = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    avatar_url = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    role = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    last_login_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
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

            migrationBuilder.CreateTable(
                name: "menu_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    category_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    price = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    available = table.Column<bool>(type: "boolean", nullable: false),
                    visibility = table.Column<int>(type: "integer", nullable: false),
                    components = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    image_url = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_menu_items", x => x.id);
                    table.ForeignKey(
                        name: "FK_menu_items_menu_categories_category_id",
                        column: x => x.category_id,
                        principalTable: "menu_categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "order_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    menu_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    unit_price = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    modifiers = table.Column<JsonElement>(type: "jsonb", nullable: true),
                    total_price = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_order_items", x => x.id);
                    table.ForeignKey(
                        name: "FK_order_items_orders_order_id",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "oauth_providers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    provider = table.Column<int>(type: "integer", nullable: false),
                    provider_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    provider_email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    access_token = table.Column<string>(type: "text", nullable: false),
                    refresh_token = table.Column<string>(type: "text", nullable: true),
                    token_expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_oauth_providers", x => x.id);
                    table.ForeignKey(
                        name: "FK_oauth_providers_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_analytics",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    session_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    event_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    event_data = table.Column<string>(type: "jsonb", nullable: false, defaultValue: "{}"),
                    ip_address = table.Column<IPAddress>(type: "inet", nullable: true),
                    user_agent = table.Column<string>(type: "text", nullable: true),
                    referrer_url = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_analytics", x => x.id);
                    table.ForeignKey(
                        name: "FK_user_analytics_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "idempotency_keys_key_key",
                table: "idempotency_keys",
                column: "key",
                unique: true);

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

            migrationBuilder.CreateIndex(
                name: "IX_menu_categories_name",
                table: "menu_categories",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_menu_items_category_id",
                table: "menu_items",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "idx_oauth_providers_user_id",
                table: "oauth_providers",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_oauth_providers_provider_provider_id",
                table: "oauth_providers",
                columns: new[] { "provider", "provider_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_order_items_order_id",
                table: "order_items",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "idx_payments_order_id",
                table: "payments",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "idx_payments_user_id",
                table: "payments",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "idx_user_analytics_created_at",
                table: "user_analytics",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "idx_user_analytics_session_id",
                table: "user_analytics",
                column: "session_id");

            migrationBuilder.CreateIndex(
                name: "idx_user_analytics_user_id",
                table: "user_analytics",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "idx_users_created_at",
                table: "users",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "idx_users_is_active",
                table: "users",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "IX_users_email",
                table: "users",
                column: "email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "idempotency_keys");

            migrationBuilder.DropTable(
                name: "loyalty_redemptions");

            migrationBuilder.DropTable(
                name: "loyalty_transactions");

            migrationBuilder.DropTable(
                name: "menu_items");

            migrationBuilder.DropTable(
                name: "oauth_providers");

            migrationBuilder.DropTable(
                name: "order_items");

            migrationBuilder.DropTable(
                name: "payments");

            migrationBuilder.DropTable(
                name: "user_analytics");

            migrationBuilder.DropTable(
                name: "loyalty_accounts");

            migrationBuilder.DropTable(
                name: "menu_categories");

            migrationBuilder.DropTable(
                name: "orders");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
