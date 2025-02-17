using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Agience.Authority.Identity.Migrations
{
    /// <inheritdoc />
    public partial class migration_404018659 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Functions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Instruction = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Functions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "People",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ProviderId = table.Column<string>(type: "text", nullable: true),
                    ProviderPersonId = table.Column<string>(type: "text", nullable: true),
                    LastLogin = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FirstName = table.Column<string>(type: "text", nullable: true),
                    LastName = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_People", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Parameters",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    FunctionId = table.Column<string>(type: "text", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: true),
                    InputFunctionId = table.Column<string>(type: "text", nullable: true),
                    OutputFunctionId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Parameters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Parameters_Functions_FunctionId",
                        column: x => x.FunctionId,
                        principalTable: "Functions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Parameters_Functions_InputFunctionId",
                        column: x => x.InputFunctionId,
                        principalTable: "Functions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Parameters_Functions_OutputFunctionId",
                        column: x => x.OutputFunctionId,
                        principalTable: "Functions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Authorizers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    OwnerId = table.Column<string>(type: "text", nullable: true),
                    Visibility = table.Column<string>(type: "text", nullable: false, defaultValue: "Private"),
                    ClientId = table.Column<string>(type: "text", nullable: true),
                    ClientSecret = table.Column<string>(type: "text", nullable: true),
                    AuthUri = table.Column<string>(type: "text", nullable: true),
                    TokenUri = table.Column<string>(type: "text", nullable: true),
                    Scopes = table.Column<string>(type: "text", nullable: true),
                    AuthType = table.Column<string>(type: "text", nullable: false, defaultValue: "Public")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Authorizers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Authorizers_People_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "People",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Connections",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    OwnerId = table.Column<string>(type: "text", nullable: true),
                    Visibility = table.Column<string>(type: "text", nullable: false, defaultValue: "Private"),
                    ResourceUri = table.Column<string>(type: "text", nullable: true),
                    Scopes = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Connections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Connections_People_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "People",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Hosts",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    OwnerId = table.Column<string>(type: "text", nullable: true),
                    Visibility = table.Column<string>(type: "text", nullable: false, defaultValue: "Private"),
                    RedirectUris = table.Column<string>(type: "text", nullable: true),
                    PostLogoutUris = table.Column<string>(type: "text", nullable: true),
                    Scopes = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hosts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Hosts_People_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "People",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Plugins",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    OwnerId = table.Column<string>(type: "text", nullable: true),
                    Visibility = table.Column<string>(type: "text", nullable: false, defaultValue: "Private"),
                    UniqueName = table.Column<string>(type: "text", nullable: true),
                    PluginProvider = table.Column<string>(type: "text", nullable: false, defaultValue: "Prompt"),
                    PluginSource = table.Column<string>(type: "text", nullable: false, defaultValue: "UserDefined")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Plugins", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Plugins_People_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "People",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Topics",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    OwnerId = table.Column<string>(type: "text", nullable: true),
                    Visibility = table.Column<string>(type: "text", nullable: false, defaultValue: "Private"),
                    Address = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Topics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Topics_People_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "People",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ConnectionAuthorizers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    ConnectionId = table.Column<string>(type: "text", nullable: true),
                    AuthorizerId = table.Column<string>(type: "text", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConnectionAuthorizers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConnectionAuthorizers_Authorizers_AuthorizerId",
                        column: x => x.AuthorizerId,
                        principalTable: "Authorizers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ConnectionAuthorizers_Connections_ConnectionId",
                        column: x => x.ConnectionId,
                        principalTable: "Connections",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "FunctionConnections",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FunctionId = table.Column<string>(type: "text", nullable: true),
                    ConnectionId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FunctionConnections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FunctionConnections_Connections_ConnectionId",
                        column: x => x.ConnectionId,
                        principalTable: "Connections",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FunctionConnections_Functions_FunctionId",
                        column: x => x.FunctionId,
                        principalTable: "Functions",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Agents",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    OwnerId = table.Column<string>(type: "text", nullable: true),
                    Persona = table.Column<string>(type: "text", nullable: true),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    ExecutiveFunctionId = table.Column<string>(type: "text", nullable: true),
                    AutoStartFunctionId = table.Column<string>(type: "text", nullable: true),
                    OnAutoStartFunctionComplete = table.Column<int>(type: "integer", nullable: true),
                    HostId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Agents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Agents_Functions_AutoStartFunctionId",
                        column: x => x.AutoStartFunctionId,
                        principalTable: "Functions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Agents_Functions_ExecutiveFunctionId",
                        column: x => x.ExecutiveFunctionId,
                        principalTable: "Functions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Agents_Hosts_HostId",
                        column: x => x.HostId,
                        principalTable: "Hosts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Agents_People_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "People",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Keys",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    SaltedValue = table.Column<string>(type: "text", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: false),
                    HostId = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Keys", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Keys_Hosts_HostId",
                        column: x => x.HostId,
                        principalTable: "Hosts",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "HostPlugins",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    HostId = table.Column<string>(type: "text", nullable: true),
                    PluginId = table.Column<string>(type: "text", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HostPlugins", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HostPlugins_Hosts_HostId",
                        column: x => x.HostId,
                        principalTable: "Hosts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_HostPlugins_Plugins_PluginId",
                        column: x => x.PluginId,
                        principalTable: "Plugins",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PluginFunctions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    PluginId = table.Column<string>(type: "text", nullable: true),
                    FunctionId = table.Column<string>(type: "text", nullable: true),
                    IsRoot = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PluginFunctions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PluginFunctions_Functions_FunctionId",
                        column: x => x.FunctionId,
                        principalTable: "Functions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PluginFunctions_Plugins_PluginId",
                        column: x => x.PluginId,
                        principalTable: "Plugins",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AgentLogEntries",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    AgentId = table.Column<string>(type: "text", nullable: false),
                    LogText = table.Column<string>(type: "text", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgentLogEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AgentLogEntries_Agents_AgentId",
                        column: x => x.AgentId,
                        principalTable: "Agents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AgentPlugins",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    AgentId = table.Column<string>(type: "text", nullable: false),
                    PluginId = table.Column<string>(type: "text", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgentPlugins", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AgentPlugins_Agents_AgentId",
                        column: x => x.AgentId,
                        principalTable: "Agents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AgentPlugins_Plugins_PluginId",
                        column: x => x.PluginId,
                        principalTable: "Plugins",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AgentTopics",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    AgentId = table.Column<string>(type: "text", nullable: true),
                    TopicId = table.Column<string>(type: "text", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgentTopics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AgentTopics_Agents_AgentId",
                        column: x => x.AgentId,
                        principalTable: "Agents",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AgentTopics_Topics_TopicId",
                        column: x => x.TopicId,
                        principalTable: "Topics",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Credentials",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AgentId = table.Column<string>(type: "text", nullable: true),
                    ConnectionId = table.Column<string>(type: "text", nullable: true),
                    AuthorizerId = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false, defaultValue: "NoAuthorizer"),
                    RefreshToken = table.Column<string>(type: "text", nullable: true),
                    AccessToken = table.Column<string>(type: "text", nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Credentials", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Credentials_Agents_AgentId",
                        column: x => x.AgentId,
                        principalTable: "Agents",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Credentials_Authorizers_AuthorizerId",
                        column: x => x.AuthorizerId,
                        principalTable: "Authorizers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Credentials_Connections_ConnectionId",
                        column: x => x.ConnectionId,
                        principalTable: "Connections",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AgentLogEntries_AgentId",
                table: "AgentLogEntries",
                column: "AgentId");

            migrationBuilder.CreateIndex(
                name: "IX_AgentPlugins_AgentId_PluginId",
                table: "AgentPlugins",
                columns: new[] { "AgentId", "PluginId" });

            migrationBuilder.CreateIndex(
                name: "IX_AgentPlugins_PluginId",
                table: "AgentPlugins",
                column: "PluginId");

            migrationBuilder.CreateIndex(
                name: "IX_Agents_AutoStartFunctionId",
                table: "Agents",
                column: "AutoStartFunctionId");

            migrationBuilder.CreateIndex(
                name: "IX_Agents_ExecutiveFunctionId",
                table: "Agents",
                column: "ExecutiveFunctionId");

            migrationBuilder.CreateIndex(
                name: "IX_Agents_HostId",
                table: "Agents",
                column: "HostId");

            migrationBuilder.CreateIndex(
                name: "IX_Agents_OwnerId",
                table: "Agents",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_AgentTopics_AgentId_TopicId",
                table: "AgentTopics",
                columns: new[] { "AgentId", "TopicId" });

            migrationBuilder.CreateIndex(
                name: "IX_AgentTopics_TopicId",
                table: "AgentTopics",
                column: "TopicId");

            migrationBuilder.CreateIndex(
                name: "IX_Authorizers_OwnerId",
                table: "Authorizers",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_ConnectionAuthorizers_AuthorizerId",
                table: "ConnectionAuthorizers",
                column: "AuthorizerId");

            migrationBuilder.CreateIndex(
                name: "IX_ConnectionAuthorizers_ConnectionId_AuthorizerId",
                table: "ConnectionAuthorizers",
                columns: new[] { "ConnectionId", "AuthorizerId" });

            migrationBuilder.CreateIndex(
                name: "IX_Connections_OwnerId",
                table: "Connections",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Credentials_AgentId",
                table: "Credentials",
                column: "AgentId");

            migrationBuilder.CreateIndex(
                name: "IX_Credentials_AuthorizerId",
                table: "Credentials",
                column: "AuthorizerId");

            migrationBuilder.CreateIndex(
                name: "IX_Credentials_ConnectionId",
                table: "Credentials",
                column: "ConnectionId");

            migrationBuilder.CreateIndex(
                name: "IX_FunctionConnections_ConnectionId",
                table: "FunctionConnections",
                column: "ConnectionId");

            migrationBuilder.CreateIndex(
                name: "IX_FunctionConnections_FunctionId_ConnectionId",
                table: "FunctionConnections",
                columns: new[] { "FunctionId", "ConnectionId" });

            migrationBuilder.CreateIndex(
                name: "IX_HostPlugins_HostId_PluginId",
                table: "HostPlugins",
                columns: new[] { "HostId", "PluginId" });

            migrationBuilder.CreateIndex(
                name: "IX_HostPlugins_PluginId",
                table: "HostPlugins",
                column: "PluginId");

            migrationBuilder.CreateIndex(
                name: "IX_Hosts_OwnerId",
                table: "Hosts",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Keys_HostId",
                table: "Keys",
                column: "HostId");

            migrationBuilder.CreateIndex(
                name: "IX_Parameters_FunctionId",
                table: "Parameters",
                column: "FunctionId");

            migrationBuilder.CreateIndex(
                name: "IX_Parameters_InputFunctionId",
                table: "Parameters",
                column: "InputFunctionId");

            migrationBuilder.CreateIndex(
                name: "IX_Parameters_OutputFunctionId",
                table: "Parameters",
                column: "OutputFunctionId");

            migrationBuilder.CreateIndex(
                name: "IX_PluginFunctions_FunctionId",
                table: "PluginFunctions",
                column: "FunctionId");

            migrationBuilder.CreateIndex(
                name: "IX_PluginFunctions_PluginId_FunctionId",
                table: "PluginFunctions",
                columns: new[] { "PluginId", "FunctionId" });

            migrationBuilder.CreateIndex(
                name: "IX_Plugins_OwnerId",
                table: "Plugins",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Topics_OwnerId",
                table: "Topics",
                column: "OwnerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AgentLogEntries");

            migrationBuilder.DropTable(
                name: "AgentPlugins");

            migrationBuilder.DropTable(
                name: "AgentTopics");

            migrationBuilder.DropTable(
                name: "ConnectionAuthorizers");

            migrationBuilder.DropTable(
                name: "Credentials");

            migrationBuilder.DropTable(
                name: "FunctionConnections");

            migrationBuilder.DropTable(
                name: "HostPlugins");

            migrationBuilder.DropTable(
                name: "Keys");

            migrationBuilder.DropTable(
                name: "Parameters");

            migrationBuilder.DropTable(
                name: "PluginFunctions");

            migrationBuilder.DropTable(
                name: "Topics");

            migrationBuilder.DropTable(
                name: "Agents");

            migrationBuilder.DropTable(
                name: "Authorizers");

            migrationBuilder.DropTable(
                name: "Connections");

            migrationBuilder.DropTable(
                name: "Plugins");

            migrationBuilder.DropTable(
                name: "Functions");

            migrationBuilder.DropTable(
                name: "Hosts");

            migrationBuilder.DropTable(
                name: "People");
        }
    }
}
