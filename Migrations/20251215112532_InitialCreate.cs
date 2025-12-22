using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace HomeRadar.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BuildingTypes",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(maxLength: 50, nullable: false),
                    Description = table.Column<string>(maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuildingTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Districts",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(maxLength: 100, nullable: false),
                    City = table.Column<string>(maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Districts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Features",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(maxLength: 50, nullable: false),
                    Description = table.Column<string>(maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Features", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Email = table.Column<string>(maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(maxLength: 255, nullable: false),
                    FirstName = table.Column<string>(maxLength: 50, nullable: false),
                    LastName = table.Column<string>(maxLength: 50, nullable: false),
                    Role = table.Column<string>(maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Listings",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DistrictId = table.Column<int>(nullable: false),
                    BuildingTypeId = table.Column<int>(nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SquareMeters = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    RoomCount = table.Column<int>(nullable: false),
                    SalonCount = table.Column<int>(nullable: false),
                    BathroomCount = table.Column<int>(nullable: true),
                    Floor = table.Column<string>(maxLength: 50, nullable: true),
                    BuildingAge = table.Column<int>(nullable: false),
                    Aidat = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    HeatingType = table.Column<string>(maxLength: 50, nullable: true),
                    Direction = table.Column<string>(maxLength: 50, nullable: true),
                    BuildingStatus = table.Column<string>(maxLength: 50, nullable: true),
                    UsageStatus = table.Column<string>(maxLength: 50, nullable: true),
                    DeedStatus = table.Column<string>(maxLength: 50, nullable: true),
                    FurnitureStatus = table.Column<string>(maxLength: 50, nullable: true),
                    HasBalcony = table.Column<bool>(nullable: true),
                    HasElevator = table.Column<bool>(nullable: true),
                    HasGarage = table.Column<bool>(nullable: true),
                    IsInComplex = table.Column<bool>(nullable: true),
                    HasSecurity = table.Column<bool>(nullable: true),
                    IsCreditSuitable = table.Column<bool>(nullable: true),
                    IsExchangeable = table.Column<bool>(nullable: true),
                    Neighborhood = table.Column<string>(maxLength: 200, nullable: true),
                    ListingDate = table.Column<DateTime>(nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Listings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Listings_BuildingTypes_BuildingTypeId",
                        column: x => x.BuildingTypeId,
                        principalTable: "BuildingTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Listings_Districts_DistrictId",
                        column: x => x.DistrictId,
                        principalTable: "Districts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ListingFeatures",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ListingId = table.Column<int>(nullable: false),
                    FeatureId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ListingFeatures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ListingFeatures_Features_FeatureId",
                        column: x => x.FeatureId,
                        principalTable: "Features",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ListingFeatures_Listings_ListingId",
                        column: x => x.ListingId,
                        principalTable: "Listings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Predictions",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(nullable: true),
                    ListingId = table.Column<int>(nullable: true),
                    DistrictId = table.Column<int>(nullable: false),
                    RoomCount = table.Column<int>(nullable: false),
                    SquareMeters = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    BuildingAge = table.Column<int>(nullable: false),
                    BuildingTypeId = table.Column<int>(nullable: true),
                    PredictedPriceMin = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PredictedPriceMax = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PredictedPriceAvg = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ModelName = table.Column<string>(maxLength: 50, nullable: true),
                    ConfidenceScore = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    CreatedAt = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Predictions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Predictions_BuildingTypes_BuildingTypeId",
                        column: x => x.BuildingTypeId,
                        principalTable: "BuildingTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Predictions_Districts_DistrictId",
                        column: x => x.DistrictId,
                        principalTable: "Districts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Predictions_Listings_ListingId",
                        column: x => x.ListingId,
                        principalTable: "Listings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Predictions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BuildingTypes_Name",
                table: "BuildingTypes",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Districts_Name",
                table: "Districts",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Features_Name",
                table: "Features",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ListingFeatures_FeatureId",
                table: "ListingFeatures",
                column: "FeatureId");

            migrationBuilder.CreateIndex(
                name: "IX_ListingFeatures_ListingId_FeatureId",
                table: "ListingFeatures",
                columns: new[] { "ListingId", "FeatureId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Listings_BuildingTypeId",
                table: "Listings",
                column: "BuildingTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Listings_DistrictId",
                table: "Listings",
                column: "DistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_Predictions_BuildingTypeId",
                table: "Predictions",
                column: "BuildingTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Predictions_DistrictId",
                table: "Predictions",
                column: "DistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_Predictions_ListingId",
                table: "Predictions",
                column: "ListingId");

            migrationBuilder.CreateIndex(
                name: "IX_Predictions_UserId",
                table: "Predictions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ListingFeatures");

            migrationBuilder.DropTable(
                name: "Predictions");

            migrationBuilder.DropTable(
                name: "Features");

            migrationBuilder.DropTable(
                name: "Listings");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "BuildingTypes");

            migrationBuilder.DropTable(
                name: "Districts");
        }
    }
}
