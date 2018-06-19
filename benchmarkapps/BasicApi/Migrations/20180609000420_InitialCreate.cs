using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace BasicApi.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
#if !NET461
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn)
#endif
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                        .Annotation("Sqlite:Autoincrement", true)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Pets",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
#if !NET461
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn)
#endif
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                        .Annotation("Sqlite:Autoincrement", true)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Age = table.Column<int>(nullable: false),
                    CategoryId = table.Column<int>(nullable: true),
                    HasVaccinations = table.Column<bool>(nullable: false),
                    Name = table.Column<string>(maxLength: 50, nullable: false),
                    Status = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pets_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Images",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
#if !NET461
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn)
#endif
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                        .Annotation("Sqlite:Autoincrement", true)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Url = table.Column<string>(nullable: true),
                    PetId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Images", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Images_Pets_PetId",
                        column: x => x.PetId,
                        principalTable: "Pets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Tags",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
#if !NET461
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn)
#endif
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                        .Annotation("Sqlite:Autoincrement", true)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(nullable: true),
                    PetId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tags_Pets_PetId",
                        column: x => x.PetId,
                        principalTable: "Pets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Name" },
                values: new object[] { 1, "Dogs" });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Name" },
                values: new object[] { 2, "Cats" });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Name" },
                values: new object[] { 3, "Rabbits" });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Name" },
                values: new object[] { 4, "Lions" });

            migrationBuilder.InsertData(
                table: "Pets",
                columns: new[] { "Id", "Age", "CategoryId", "HasVaccinations", "Name", "Status" },
                values: new object[] { 1, 1, 1, true, "Dogs1", "available" });

            migrationBuilder.InsertData(
                table: "Pets",
                columns: new[] { "Id", "Age", "CategoryId", "HasVaccinations", "Name", "Status" },
                values: new object[] { 2, 1, 1, true, "Dogs2", "available" });

            migrationBuilder.InsertData(
                table: "Pets",
                columns: new[] { "Id", "Age", "CategoryId", "HasVaccinations", "Name", "Status" },
                values: new object[] { 3, 1, 1, true, "Dogs3", "available" });

            migrationBuilder.InsertData(
                table: "Pets",
                columns: new[] { "Id", "Age", "CategoryId", "HasVaccinations", "Name", "Status" },
                values: new object[] { 4, 1, 2, true, "Cats1", "available" });

            migrationBuilder.InsertData(
                table: "Pets",
                columns: new[] { "Id", "Age", "CategoryId", "HasVaccinations", "Name", "Status" },
                values: new object[] { 5, 1, 2, true, "Cats2", "available" });

            migrationBuilder.InsertData(
                table: "Pets",
                columns: new[] { "Id", "Age", "CategoryId", "HasVaccinations", "Name", "Status" },
                values: new object[] { 6, 1, 2, true, "Cats3", "available" });

            migrationBuilder.InsertData(
                table: "Pets",
                columns: new[] { "Id", "Age", "CategoryId", "HasVaccinations", "Name", "Status" },
                values: new object[] { 7, 1, 3, true, "Rabbits1", "available" });

            migrationBuilder.InsertData(
                table: "Pets",
                columns: new[] { "Id", "Age", "CategoryId", "HasVaccinations", "Name", "Status" },
                values: new object[] { 8, 1, 3, true, "Rabbits2", "available" });

            migrationBuilder.InsertData(
                table: "Pets",
                columns: new[] { "Id", "Age", "CategoryId", "HasVaccinations", "Name", "Status" },
                values: new object[] { 9, 1, 3, true, "Rabbits3", "available" });

            migrationBuilder.InsertData(
                table: "Pets",
                columns: new[] { "Id", "Age", "CategoryId", "HasVaccinations", "Name", "Status" },
                values: new object[] { 10, 1, 4, true, "Lions1", "available" });

            migrationBuilder.InsertData(
                table: "Pets",
                columns: new[] { "Id", "Age", "CategoryId", "HasVaccinations", "Name", "Status" },
                values: new object[] { 11, 1, 4, true, "Lions2", "available" });

            migrationBuilder.InsertData(
                table: "Pets",
                columns: new[] { "Id", "Age", "CategoryId", "HasVaccinations", "Name", "Status" },
                values: new object[] { 12, 1, 4, true, "Lions3", "available" });

            migrationBuilder.InsertData(
                table: "Images",
                columns: new[] { "Id", "PetId", "Url" },
                values: new object[] { 1, 1, "http://example.com/pets/1_1.png" });

            migrationBuilder.InsertData(
                table: "Images",
                columns: new[] { "Id", "PetId", "Url" },
                values: new object[] { 2, 2, "http://example.com/pets/2_1.png" });

            migrationBuilder.InsertData(
                table: "Images",
                columns: new[] { "Id", "PetId", "Url" },
                values: new object[] { 11, 11, "http://example.com/pets/11_1.png" });

            migrationBuilder.InsertData(
                table: "Images",
                columns: new[] { "Id", "PetId", "Url" },
                values: new object[] { 3, 3, "http://example.com/pets/3_1.png" });

            migrationBuilder.InsertData(
                table: "Images",
                columns: new[] { "Id", "PetId", "Url" },
                values: new object[] { 4, 4, "http://example.com/pets/4_1.png" });

            migrationBuilder.InsertData(
                table: "Images",
                columns: new[] { "Id", "PetId", "Url" },
                values: new object[] { 10, 10, "http://example.com/pets/10_1.png" });

            migrationBuilder.InsertData(
                table: "Images",
                columns: new[] { "Id", "PetId", "Url" },
                values: new object[] { 5, 5, "http://example.com/pets/5_1.png" });

            migrationBuilder.InsertData(
                table: "Images",
                columns: new[] { "Id", "PetId", "Url" },
                values: new object[] { 6, 6, "http://example.com/pets/6_1.png" });

            migrationBuilder.InsertData(
                table: "Images",
                columns: new[] { "Id", "PetId", "Url" },
                values: new object[] { 12, 12, "http://example.com/pets/12_1.png" });

            migrationBuilder.InsertData(
                table: "Images",
                columns: new[] { "Id", "PetId", "Url" },
                values: new object[] { 7, 7, "http://example.com/pets/7_1.png" });

            migrationBuilder.InsertData(
                table: "Images",
                columns: new[] { "Id", "PetId", "Url" },
                values: new object[] { 9, 9, "http://example.com/pets/9_1.png" });

            migrationBuilder.InsertData(
                table: "Images",
                columns: new[] { "Id", "PetId", "Url" },
                values: new object[] { 8, 8, "http://example.com/pets/8_1.png" });

            migrationBuilder.InsertData(
                table: "Tags",
                columns: new[] { "Id", "Name", "PetId" },
                values: new object[] { 11, "Tag1", 11 });

            migrationBuilder.InsertData(
                table: "Tags",
                columns: new[] { "Id", "Name", "PetId" },
                values: new object[] { 10, "Tag1", 10 });

            migrationBuilder.InsertData(
                table: "Tags",
                columns: new[] { "Id", "Name", "PetId" },
                values: new object[] { 9, "Tag1", 9 });

            migrationBuilder.InsertData(
                table: "Tags",
                columns: new[] { "Id", "Name", "PetId" },
                values: new object[] { 6, "Tag1", 6 });

            migrationBuilder.InsertData(
                table: "Tags",
                columns: new[] { "Id", "Name", "PetId" },
                values: new object[] { 7, "Tag1", 7 });

            migrationBuilder.InsertData(
                table: "Tags",
                columns: new[] { "Id", "Name", "PetId" },
                values: new object[] { 5, "Tag1", 5 });

            migrationBuilder.InsertData(
                table: "Tags",
                columns: new[] { "Id", "Name", "PetId" },
                values: new object[] { 4, "Tag1", 4 });

            migrationBuilder.InsertData(
                table: "Tags",
                columns: new[] { "Id", "Name", "PetId" },
                values: new object[] { 3, "Tag1", 3 });

            migrationBuilder.InsertData(
                table: "Tags",
                columns: new[] { "Id", "Name", "PetId" },
                values: new object[] { 2, "Tag1", 2 });

            migrationBuilder.InsertData(
                table: "Tags",
                columns: new[] { "Id", "Name", "PetId" },
                values: new object[] { 1, "Tag1", 1 });

            migrationBuilder.InsertData(
                table: "Tags",
                columns: new[] { "Id", "Name", "PetId" },
                values: new object[] { 8, "Tag1", 8 });

            migrationBuilder.InsertData(
                table: "Tags",
                columns: new[] { "Id", "Name", "PetId" },
                values: new object[] { 12, "Tag1", 12 });

            migrationBuilder.CreateIndex(
                name: "IX_Images_PetId",
                table: "Images",
                column: "PetId");

            migrationBuilder.CreateIndex(
                name: "IX_Pets_CategoryId",
                table: "Pets",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_PetId",
                table: "Tags",
                column: "PetId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Images");

            migrationBuilder.DropTable(
                name: "Tags");

            migrationBuilder.DropTable(
                name: "Pets");

            migrationBuilder.DropTable(
                name: "Categories");
        }
    }
}
