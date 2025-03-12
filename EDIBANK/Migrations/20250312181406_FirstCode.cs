using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EDIBANK.Migrations
{
    /// <inheritdoc />
    public partial class FirstCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DisplayName = table.Column<string>(type: "varchar(256)", nullable: false),
                    EDIId = table.Column<string>(type: "varchar(15)", unicode: false, nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "auditoria",
                columns: table => new
                {
                    id_auditoria = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    fec_auditoria = table.Column<DateTime>(type: "datetime2", nullable: false),
                    login_usuario = table.Column<string>(type: "varchar(192)", nullable: false),
                    ip_remota = table.Column<string>(type: "varchar(39)", unicode: false, nullable: false),
                    modulo = table.Column<string>(type: "varchar(192)", nullable: false),
                    operacion = table.Column<string>(type: "varchar(48)", nullable: false),
                    comentarios = table.Column<string>(type: "varchar(768)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_auditoria", x => x.id_auditoria);
                });

            migrationBuilder.CreateTable(
                name: "edi",
                columns: table => new
                {
                    id_edi = table.Column<string>(type: "varchar(15)", unicode: false, nullable: false),
                    des_edi = table.Column<string>(type: "varchar(50)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_edi", x => x.id_edi);
                });

            migrationBuilder.CreateTable(
                name: "hist_intercambios",
                columns: table => new
                {
                    id_tracking = table.Column<string>(type: "char(17)", unicode: false, nullable: false, comment: "YYYYMMDDhhmmsssss"),
                    fec_carga = table.Column<DateTime>(type: "datetime2", nullable: false),
                    fec_descarga = table.Column<DateTime>(type: "datetime2", nullable: true),
                    nro_intercambio = table.Column<string>(type: "varchar(50)", nullable: false),
                    tip_intercambio = table.Column<byte>(type: "tinyint", nullable: false),
                    edi_envia = table.Column<string>(type: "varchar(15)", unicode: false, nullable: false),
                    edi_recibe = table.Column<string>(type: "varchar(15)", unicode: false, nullable: false),
                    tip_documento = table.Column<string>(type: "varchar(50)", nullable: true),
                    ori_archivo = table.Column<string>(type: "varchar(50)", nullable: false),
                    int_archivo = table.Column<string>(type: "varchar(59)", nullable: false),
                    ruta_archivo = table.Column<string>(type: "varchar(200)", nullable: false),
                    tamano = table.Column<long>(type: "bigint", nullable: false),
                    status = table.Column<byte>(type: "tinyint", nullable: false),
                    es_recarga = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hist_intercambios", x => x.id_tracking);
                    table.CheckConstraint("CK_status", "[status] IN (0, 1, 2, 3)");
                    table.CheckConstraint("CK_tip_intercambio", "[tip_intercambio] IN (0, 1, 2)");
                });

            migrationBuilder.CreateTable(
                name: "intercambios",
                columns: table => new
                {
                    id_tracking = table.Column<string>(type: "char(17)", unicode: false, nullable: false, comment: "YYYYMMDDhhmmsssss"),
                    fec_carga = table.Column<DateTime>(type: "datetime2", nullable: false),
                    fec_descarga = table.Column<DateTime>(type: "datetime2", nullable: true),
                    nro_intercambio = table.Column<string>(type: "varchar(50)", nullable: false),
                    tip_intercambio = table.Column<byte>(type: "tinyint", nullable: false),
                    edi_envia = table.Column<string>(type: "varchar(15)", unicode: false, nullable: false),
                    edi_recibe = table.Column<string>(type: "varchar(15)", unicode: false, nullable: false),
                    tip_documento = table.Column<string>(type: "varchar(50)", nullable: true),
                    ori_archivo = table.Column<string>(type: "varchar(50)", nullable: false),
                    int_archivo = table.Column<string>(type: "varchar(59)", nullable: false),
                    ruta_archivo = table.Column<string>(type: "varchar(200)", nullable: false),
                    tamano = table.Column<long>(type: "bigint", nullable: false),
                    status = table.Column<byte>(type: "tinyint", nullable: false),
                    es_recarga = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_intercambios", x => x.id_tracking);
                    table.CheckConstraint("CK_status1", "[status] IN (0, 1, 2, 3)");
                    table.CheckConstraint("CK_tip_intercambio1", "[tip_intercambio] IN (0, 1, 2)");
                });

            migrationBuilder.CreateTable(
                name: "parametros",
                columns: table => new
                {
                    email_svc = table.Column<string>(type: "varchar(192)", nullable: false),
                    des_svc = table.Column<string>(type: "varchar(192)", nullable: false),
                    host_smtp = table.Column<string>(type: "varchar(48)", nullable: false),
                    port_smtp = table.Column<int>(type: "int", nullable: false),
                    pass_smtp = table.Column<string>(type: "varchar(48)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_parametros", x => x.email_svc);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_edi_des_edi",
                table: "edi",
                column: "des_edi",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_hist_intercambios_fec_carga",
                table: "hist_intercambios",
                column: "fec_carga");

            migrationBuilder.CreateIndex(
                name: "IX_intercambios_fec_carga",
                table: "intercambios",
                column: "fec_carga");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "auditoria");

            migrationBuilder.DropTable(
                name: "edi");

            migrationBuilder.DropTable(
                name: "hist_intercambios");

            migrationBuilder.DropTable(
                name: "intercambios");

            migrationBuilder.DropTable(
                name: "parametros");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");
        }
    }
}
